using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace ClashRoyaleUI
{
    // Uses authored components only; no UI objects or resources are created here.
    [DisallowMultipleComponent]
    public sealed class UIBackendClient : MonoBehaviour
    {
        [SerializeField] private string baseUrl = "http://127.0.0.1:18765";
        [SerializeField] private string accessToken;
        [SerializeField] private TMP_Text feedback;
        public BackendState State { get; private set; }
        public bool Busy { get; private set; }
        public string LastError { get; private set; }
        public string LastResponse { get; private set; }
        public bool UsesDefaultLocalService => baseUrl.TrimEnd('/') == "http://127.0.0.1:18765";
        public event Action<BackendState> Changed;
        public event Action<bool> BusyChanged;
        private float feedbackUntil;
        private float nextRefresh;
        private float startupGraceUntil;
        private string connectionFeedback;
        private bool useLocalCredential;
        [Serializable] private sealed class LocalCredential { public string client_token; }
        private void Awake()
        {
            startupGraceUntil = Time.realtimeSinceStartup + (Application.isEditor && UsesDefaultLocalService ? 10 : 0);
            useLocalCredential = string.IsNullOrEmpty(accessToken);
            ReadLocalCredential();
        }
        private void ReadLocalCredential()
        {
#if UNITY_EDITOR
            if (useLocalCredential && UsesDefaultLocalService)
            {
                string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Backend/.local/credentials.json"));
                try
                {
                    if (System.IO.File.Exists(path)) accessToken = JsonUtility.FromJson<LocalCredential>(System.IO.File.ReadAllText(path))?.client_token;
                }
                catch (System.IO.IOException) { } // The first server launch may still be writing credentials.
                catch (ArgumentException) { }
            }
#endif
        }
        private void Start() => StartCoroutine(StartSession());
        private IEnumerator StartSession()
        {
            yield return LoadState();
            while(State==null)
            {
                yield return new WaitForSecondsRealtime(2);
                while(Busy)yield return null;
                yield return LoadState();
            }
            // This scene starts at the home page; an interrupted training match has no live simulation to resume.
            while(Busy)yield return null;
            yield return Command("/v1/training/leave","{}",null);
        }
        private void Update()
        {
            if (feedback && feedback.gameObject.activeSelf && Time.unscaledTime > feedbackUntil) feedback.gameObject.SetActive(false);
            if (!Busy && Time.unscaledTime >= nextRefresh) Refresh();
        }
        public void Refresh() { if (!Busy && isActiveAndEnabled) StartCoroutine(LoadState()); }
        private void SetBusy(bool busy) { Busy = busy; BusyChanged?.Invoke(busy); }
        public void Notify(string message)
        {
            if (!feedback) return;
            feedback.text = message; feedback.gameObject.SetActive(true); feedbackUntil = Time.unscaledTime + 4;
        }
        public void Send(string path, string json = "{}", Action<BackendReply> completed = null)
        {
            if (Busy) return;
            StartCoroutine(Command(path, json, completed));
        }
        private UnityWebRequest Request(string path, string method, string json, string key = null)
        {
            ReadLocalCredential();
            var request = new UnityWebRequest(baseUrl.TrimEnd('/') + path, method);
            request.downloadHandler = new DownloadHandlerBuffer(); request.timeout = 8;
            if (method != "GET") { request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)); request.SetRequestHeader("Content-Type", "application/json"); }
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            if (key != null) request.SetRequestHeader("Idempotency-Key", key);
            return request;
        }
        private IEnumerator LoadState()
        {
            nextRefresh = Time.unscaledTime + 15;
            SetBusy(true);
            using (var request = Request("/v1/state", "GET", null))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    BackendState next = null;
                    try { next = JsonUtility.FromJson<BackendState>(request.downloadHandler.text); }
                    catch (Exception) { LastError = "服务器返回的数据无法读取"; }
                    if (next != null && next.ok && next.player != null)
                    {
                        State = next; LastError = null;
                        if (connectionFeedback != null && feedback && feedback.text == connectionFeedback) feedback.gameObject.SetActive(false);
                        connectionFeedback = null;
                        Changed?.Invoke(State);
                    }
                    else Notify(LastError ?? "服务器返回的数据不完整");
                }
                else
                {
                    LastError = ReadError(request);
                    nextRefresh = Time.unscaledTime + 2;
                    bool starting = request.result == UnityWebRequest.Result.ConnectionError && State == null && Time.realtimeSinceStartup < startupGraceUntil;
                    if (!starting) { connectionFeedback = LastError; Notify(LastError); }
                }
            }
            SetBusy(false);
        }
        private IEnumerator Command(string path, string json, Action<BackendReply> completed)
        {
            SetBusy(true); string key = Guid.NewGuid().ToString("N"); BackendReply reply = null;
            // A lost response may already have committed; both attempts use the same identity.
            for (int attempt = 0; attempt < 2; attempt++)
            {
                using (var request = Request(path, "POST", json, key))
                {
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.ConnectionError && attempt == 0) continue;
                    LastResponse = request.downloadHandler.text;
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try { reply = JsonUtility.FromJson<BackendReply>(LastResponse); }
                        catch (Exception) { LastError = "服务器返回的数据无法读取"; }
                        if (reply != null && reply.ok) { LastError = null; if (!string.IsNullOrEmpty(reply.result?.message)) Notify(reply.result.message); }
                    }
                    else { LastError = ReadError(request); Notify(LastError); }
                    break;
                }
            }
            if (reply != null && reply.ok) completed?.Invoke(reply);
            string commandError = reply != null && reply.ok ? null : LastError;
            yield return LoadState();
            if (commandError != null) LastError = commandError;
        }
        private static string ReadError(UnityWebRequest request)
        {
            try { var reply = JsonUtility.FromJson<BackendReply>(request.downloadHandler.text); if (!string.IsNullOrEmpty(reply?.error)) return reply.error; } catch (Exception) { }
            return "暂时无法连接本机服务，请启动后端后重试";
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ClashRoyaleUI.Editor
{
    // Editor-only local authority simulator. Never included in the mobile player.
    public sealed class LocalBackendWindow : EditorWindow
    {
        [Serializable] private sealed class Credentials { public string client_token,admin_token; }
        private BackendState state;
        private UnityWebRequest request;
        private string message="启动本机服务后刷新数据。";
        private int selected;
        private Vector2 scroll;
        private string facts="{\"mode\":\"ladder\",\"won\":true,\"deck\":{\"cards\":[\"c1\",\"c2\",\"c3\",\"c4\",\"c5\",\"c6\",\"c7\",\"c8\"],\"saved\":true,\"spells\":2,\"buildings\":1,\"elixir\":3,\"non_main\":true},\"stats\":{\"towers\":1,\"troop_plays\":6,\"spell_plays\":2,\"building_plays\":2}}";
        [MenuItem("Clash Royale/本机后端联调")]
        public static void Open() { var w=GetWindow<LocalBackendWindow>("本机后端联调");w.minSize=new Vector2(430,500); }
        private static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        private void OnDisable(){request?.Abort();request?.Dispose();request=null;}
        private void StartBackend()
        {
            LocalBackendService.StartIfNeeded();message="启动中，稍后点击刷新。";
        }
        private void Call(string path,string json=null,bool admin=false)
        {
            if(request!=null)return;
            var file=Path.Combine(Root,"Backend/.local/credentials.json");
            if(!File.Exists(file)){message="请先启动本机后端。";return;}
            var auth=JsonUtility.FromJson<Credentials>(File.ReadAllText(file));
            request=new UnityWebRequest("http://127.0.0.1:18765"+path,json==null?"GET":"POST");request.timeout=10;request.downloadHandler=new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization","Bearer "+(admin?auth.admin_token:auth.client_token));
            if(json!=null){request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Idempotency-Key",Guid.NewGuid().ToString("N"));}
            request.SendWebRequest().completed+=_=>
            {
                if(request==null)return;
                bool ok=request.result==UnityWebRequest.Result.Success;message=request.downloadHandler.text;
                if(ok&&path=="/v1/state")state=JsonUtility.FromJson<BackendState>(message);
                request.Dispose();request=null;Repaint();
                if(ok&&path!="/v1/state")
                {
                    if(EditorApplication.isPlaying)UnityEngine.Object.FindFirstObjectByType<UIBackendClient>()?.Refresh();
                    Call("/v1/state");
                }
            };
        }
        private void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Excel → 本机服务 → SQLite → Unity",EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();if(GUILayout.Button("启动后端"))StartBackend();if(GUILayout.Button("打开配表"))System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Path.Combine(Application.dataPath,"Tables/ClashRoyale_V5.4.xlsx")){UseShellExecute=true});EditorGUILayout.EndHorizontal();
            using(new EditorGUI.DisabledScope(request!=null))
            {
                EditorGUILayout.BeginHorizontal();if(GUILayout.Button("重新导入 Excel"))Call("/admin/config/import","{}",true);if(GUILayout.Button("刷新数据"))Call("/v1/state");EditorGUILayout.EndHorizontal();
                if(state?.player!=null)
                {
                    var p=state.player;EditorGUILayout.LabelField($"行动点 {p.normal_ap}/{p.normal_cap}　预留 {p.reserved_ap}　储备 {p.reserve_ap}/{p.reserve_cap}");EditorGUILayout.LabelField($"宝箱 {p.pending_chests}/{p.queue_capacity}　金币 {p.gold}　战令经验 {p.pass_xp}");EditorGUILayout.LabelField($"个人贡献 {p.season_contribution}　代币 {p.tokens}　部落积分 {p.clan_score}");
                    if(state.tasks.Length>0)
                    {
                        selected=Mathf.Clamp(selected,0,state.tasks.Length-1);selected=EditorGUILayout.Popup("任务",selected,state.tasks.Select(t=>t.task_id+" · "+t.title+" · "+t.status).ToArray());var t=state.tasks[selected];
                        EditorGUILayout.HelpBox(t.description,MessageType.None);EditorGUILayout.BeginHorizontal();
                        if(t.status=="available"&&GUILayout.Button("接取"))Call("/v1/tasks/"+t.task_id+"/accept","{}");
                        if((t.status=="accepted"||t.status=="active")&&GUILayout.Button(t.forced?"取消强制":"强制激活"))Call("/v1/instances/"+t.instance_id+(t.forced?"/unforce":"/force-activate"),"{}");
                        if((t.status=="active"||t.status=="accepted")&&GUILayout.Button("取消任务"))Call("/v1/instances/"+t.instance_id+"/cancel","{}");EditorGUILayout.EndHorizontal();
                    }
                    if(string.IsNullOrEmpty(state.battle_id)){if(GUILayout.Button("开始对局，锁定已激活任务"))Call("/v1/battles/start","{}");}
                    else
                    {
                        EditorGUILayout.LabelField("本机对局事实（用于联调）",EditorStyles.boldLabel);facts=EditorGUILayout.TextArea(facts,GUILayout.MinHeight(140));
                        if(GUILayout.Button("提交这场模拟对局"))Call("/admin/battles/"+state.battle_id+"/settle","{\"facts\":"+facts+"}",true);
                    }
                }
            }
            EditorGUILayout.Space();EditorGUILayout.LabelField("服务返回",EditorStyles.boldLabel);EditorGUILayout.SelectableLabel(message,EditorStyles.textArea,GUILayout.MinHeight(110));EditorGUILayout.EndScrollView();
        }
    }
}

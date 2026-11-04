using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClashRoyaleUI.Editor
{
    // Starts the existing local service from the Editor, independently of agent shell sessions.
    [InitializeOnLoad]
    public static class LocalBackendService
    {
        private const string LastLaunchKey = "ClashRoyaleUI.Backend.LastLaunch";

        static LocalBackendService()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) StartForScene();
            };
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlaying) StartForScene();
            };
        }

        private static void StartForScene()
        {
            var client = UnityEngine.Object.FindFirstObjectByType<UIBackendClient>(FindObjectsInactive.Include);
            if (client && client.UsesDefaultLocalService) StartIfNeeded();
        }

        public static void StartIfNeeded()
        {
            // Domain reload may run both callbacks. The launcher checks health before starting Python.
            double now = (DateTime.UtcNow - new DateTime(2020, 1, 1)).TotalSeconds;
            if (double.TryParse(SessionState.GetString(LastLaunchKey, "0"),
                System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
                out double last) && now - last < 8) return;
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string launcher = Path.Combine(root, "Backend", "Start-Backend.ps1");
            if (!File.Exists(launcher))
            {
                Debug.LogWarning("本机服务启动脚本不存在：" + launcher);
                return;
            }
            try
            {
                string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "WindowsPowerShell", "v1.0", "powershell.exe");
                var info = new System.Diagnostics.ProcessStartInfo(powershell,
                    "-NoProfile -ExecutionPolicy Bypass -File \"" + launcher + "\"")
                {
                    WorkingDirectory = root,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                using (var process = System.Diagnostics.Process.Start(info)) { }
                SessionState.SetString(LastLaunchKey, now.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            catch (Exception e) { Debug.LogWarning("本机服务启动失败：" + e.Message); }
        }
    }
}

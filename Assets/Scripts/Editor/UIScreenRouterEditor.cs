using UnityEditor;
using UnityEngine;
namespace ClashRoyaleUI.Editor
{
    [CustomEditor(typeof(UIScreenRouter))]
    public sealed class UIScreenRouterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();EditorGUILayout.LabelField("界面预览（编辑模式也可切换）",EditorStyles.boldLabel);
            var router=(UIScreenRouter)target;if(router.Screens==null)return;
            foreach(var entry in router.Screens)
            {if(GUILayout.Button(entry.view?entry.view.name:entry.id.ToString())){Undo.RecordObject(router,"Preview UI screen");foreach(var e in router.Screens)if(e.view)Undo.RecordObject(e.view,"Preview UI screen");router.Show(entry.id);EditorUtility.SetDirty(router);}}
        }
    }
}

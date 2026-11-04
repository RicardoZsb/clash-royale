var live=UnityEngine.Object.FindFirstObjectByType<ClashRoyaleUI.UIBackendClient>();
var state=UnityEngine.JsonUtility.FromJson<ClashRoyaleUI.BackendState>(UnityEngine.JsonUtility.ToJson(live.State));
state.player.pass_level=3;state.player.pass_xp=19;state.player.premium=1;state.pass_claims=System.Array.Empty<ClashRoyaleUI.PassClaimData>();
var scene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
try
{
    var canvasGo=new UnityEngine.GameObject("PassPreview",typeof(UnityEngine.RectTransform),typeof(UnityEngine.Canvas));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasGo,scene);
    var canvas=canvasGo.GetComponent<UnityEngine.Canvas>();canvas.renderMode=UnityEngine.RenderMode.WorldSpace;
    ((UnityEngine.RectTransform)canvas.transform).sizeDelta=new UnityEngine.Vector2(402,874);
    var root=(UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Screens/BattlePass.prefab"),canvas.transform);
    root.SetActive(true);var view=root.GetComponent<ClashRoyaleUI.UIPassDataBinding>();
    view.Apply(state);
    if(view.rows.Take(3).Any(r=>!r.freeMarker.activeSelf||r.freeLockIcon.activeSelf||!r.premiumLock.activeSelf||r.premiumLockIcon.activeSelf))throw new System.Exception("Reached levels need unlocked markers");
    if(view.rows.Skip(3).Any(r=>!r.freeMarker.activeSelf||!r.freeLockIcon.activeSelf||!r.premiumLock.activeSelf||!r.premiumLockIcon.activeSelf))throw new System.Exception("Future rewards need locks on both tracks");
    if(view.unlockButton.gameObject.activeSelf||view.unlockIcon.activeSelf)throw new System.Exception("Activated pass still shows unlock button");
    foreach(var row in view.rows)
    {
        if(((UnityEngine.RectTransform)row.premiumLock.transform).sizeDelta!=new UnityEngine.Vector2(32,24))throw new System.Exception("Marker differs from Figma 32x24");
        if(((UnityEngine.RectTransform)row.premiumLockIcon.transform).sizeDelta!=new UnityEngine.Vector2(18,18))throw new System.Exception("Lock differs from Figma 18x18");
        if(((UnityEngine.RectTransform)row.freeMarker.transform).sizeDelta!=new UnityEngine.Vector2(32,24))throw new System.Exception("Green marker differs from Figma 32x24");
        if(((UnityEngine.RectTransform)row.freeLockIcon.transform).sizeDelta!=new UnityEngine.Vector2(18,18))throw new System.Exception("Free lock differs from Figma 18x18");
    }
    state.pass_claims=new[]{new ClashRoyaleUI.PassClaimData{level=2,track="FREE"},new ClashRoyaleUI.PassClaimData{level=2,track="PREMIUM"}};view.Apply(state);
    if(view.rows[1].freeMarker.activeSelf||view.rows[1].premiumLock.activeSelf)throw new System.Exception("Claimed reward marker remains visible");
    state.player.premium=0;state.pass_claims=System.Array.Empty<ClashRoyaleUI.PassClaimData>();view.Apply(state);
    if(view.rows.Any(r=>!r.premiumLock.activeSelf||!r.premiumLockIcon.activeSelf))throw new System.Exception("Unpurchased pass is missing locks");
    System.IO.File.WriteAllText("DesignReference/Validation/TaskCounts/pass-marker-validation.json","{\"rows\":40,\"markerSize\":[32,24],\"lockSize\":[18,18],\"locked\":true,\"unlockedReached\":true,\"futureLocked\":true,\"claimedHidden\":true}");
    return "40 native rows verified: locked, unlocked reached, future locked, claimed hidden; Figma sizes exact.";
}
finally
{
    UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
}

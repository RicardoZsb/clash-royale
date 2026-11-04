using System;
using UnityEngine;

namespace ClashRoyaleUI
{
    public enum ScreenId { BattleHome, ClanHome, Tasks, BattlePass, ChestQueue, ChestReveal, ClanShop, ClanCompetition, ClanChat, ClanChatInput, TrainingBattle }
    [DisallowMultipleComponent]
    public sealed class UIScreenRouter : MonoBehaviour
    {
        [Serializable] public class ScreenEntry { public ScreenId id; public GameObject view; }
        [SerializeField] private ScreenEntry[] screens;
        [SerializeField] private ScreenId currentScreen = ScreenId.BattleHome;
        public ScreenId CurrentScreen => currentScreen;
        public ScreenEntry[] Screens { get => screens; set => screens = value; }
        public void Show(ScreenId destination)
        {
            if (screens == null) return;
            bool found = Array.Exists(screens, s => s.id == destination && s.view != null);
            if (!found) return;
            foreach (var entry in screens) if (entry.view != null) entry.view.SetActive(entry.id == destination);
            currentScreen = destination;
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        [ContextMenu("预览 / 战斗主界面")] public void PreviewBattle() => Show(ScreenId.BattleHome);
        [ContextMenu("预览 / 部落主界面")] public void PreviewClan() => Show(ScreenId.ClanHome);
        [ContextMenu("预览 / 任务")] public void PreviewTasks() => Show(ScreenId.Tasks);
        [ContextMenu("预览 / 战令")] public void PreviewPass() => Show(ScreenId.BattlePass);
        [ContextMenu("预览 / 宝箱")] public void PreviewChests() => Show(ScreenId.ChestQueue);
    }
}

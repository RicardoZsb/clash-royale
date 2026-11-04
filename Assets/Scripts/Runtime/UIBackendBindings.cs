using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ClashRoyaleUI
{
    [DisallowMultipleComponent]
    public sealed class UIBackendBindings : MonoBehaviour
    {
        [SerializeField] private UIBackendClient client;
        [SerializeField] private UIPrototypeState prototype;
        [SerializeField] private UIActionPointReserve reserve;
        [SerializeField] private UIRuleDropdown activationOrder;
        [SerializeField] private TMP_Text[] goldLabels, gemLabels, taskCountLabels, activeCountLabels;
        [SerializeField] private TMP_Text taskActionPoints, passProgress, passLevel;
        private void Awake()
        {
            if (!client) client=GetComponent<UIBackendClient>();
            client.Changed += Apply;
            if (activationOrder) activationOrder.OnRuleSelected.AddListener(ActivateByOrder);
        }
        private void OnEnable()
        {
            if(client && client.State!=null)Apply(client.State);
        }
        private void OnDestroy() { if(client) client.Changed-=Apply; if(activationOrder) activationOrder.OnRuleSelected.RemoveListener(ActivateByOrder); }
        private static void Set(TMP_Text[] labels,string value) { if(labels!=null)foreach(var label in labels)if(label)label.text=value; }
        public void Apply(BackendState state)
        {
            var p=state.player;
            if(activationOrder)activationOrder.SetRuleWithoutNotify(p.activation_order);
            if(reserve)reserve.ApplyServerState(p);
            Set(goldLabels,p.gold.ToString());Set(gemLabels,p.gems.ToString());
            // Count the same task instances that the board displays. Active tasks are also accepted.
            int accepted=state.tasks.Count(t=>t.status=="accepted"||t.status=="active");
            int active=state.tasks.Count(t=>t.status=="active");
            Set(taskCountLabels,"×"+accepted);Set(activeCountLabels,"×"+active);
            if(taskActionPoints)taskActionPoints.text=$"{p.normal_ap}/{p.normal_cap}";
            if(passProgress)passProgress.text=$"{p.pass_xp%5} / 5";
            if(passLevel)passLevel.text=p.pass_level.ToString();
            if(prototype)
            {
                prototype.pendingChests=p.pending_chests;
                if(prototype.chestCount)prototype.chestCount.text="×"+p.pending_chests;
                prototype.passUnlocked=p.premium!=0;
                if(prototype.claimLabel)prototype.claimLabel.text="一键领取";
                if(state.clan_help!=null&&state.clan_help.Length>0&&prototype.chatMessage)
                { prototype.chatMessage.text=state.clan_help[0].message; if(prototype.chatBubble)prototype.chatBubble.SetActive(true); }
            }
        }
        public void ActivateByOrder(int order)
        {
            if(client.Busy)
            {
                if(client.State!=null&&activationOrder)activationOrder.SetRuleWithoutNotify(client.State.player.activation_order);
                client.Notify("正在同步，请稍后选择激活规则");
                return;
            }
            client.Send("/v1/tasks/activate-auto","{\"order\":"+order+"}");
        }
        public void BeginBattle()
        {
            if(client.Busy)return;
            client.Send("/v1/training/start","{}",reply=>
            {
                var battle=GetComponentInChildren<TrainingBattleController>(true);
                prototype.router.Show(ScreenId.TrainingBattle);
                battle.StartMatch(reply.result.battle_id);
            });
        }
        public void OpenChest(int index)
        {
            if(client.State==null||index<0||index>=client.State.chests.Length)return;
            var opening=GetComponentInChildren<UIChestOpening>(true);
            opening.Begin(client.State.chests[index]);
            prototype.router.Show(ScreenId.ChestReveal);
        }
        public void ClaimFreeRewards() { if(!client.Busy)StartCoroutine(ClaimAvailable()); }
        private IEnumerator ClaimAvailable()
        {
            while(client.State!=null)
            {
                var s=client.State;
                var choice=(from level in s.pass_levels from track in new[]{"FREE","PREMIUM"}
                    let reward=s.rewards.First(r=>r.reward_id==(track=="FREE"?level.free_reward_id:level.premium_reward_id))
                    where level.required_xp<=s.player.pass_xp && (track=="FREE"||s.player.premium!=0)
                    && !s.pass_claims.Any(c=>c.level==level.level&&c.track==track)
                    && (reward.ap==0||s.player.normal_ap+reward.ap<=s.player.normal_cap)
                    && (reward.chest_star==0||s.player.pending_chests+s.player.reserved_slots<s.player.queue_capacity)
                    select new{level,track}).FirstOrDefault();
                if(choice==null)yield break;
                client.Send("/v1/pass/"+choice.level.level+"/claim","{\"track\":\""+choice.track+"\"}");
                while(client.Busy)yield return null;
                if(!string.IsNullOrEmpty(client.LastError))yield break;
            }
        }
    }
}

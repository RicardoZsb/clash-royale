using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    public sealed class UIPassDataBinding : MonoBehaviour
    {
        [Serializable] public sealed class Row
        {
            public int level;
            public TMP_Text freeQuantity,premiumQuantity;
            public Image freeIcon,premiumIcon,diamond;
            public TMP_Text levelLabel;
            public GameObject freeMarker,freeLockIcon,premiumLock,premiumLockIcon;
            public Button freeClaim,premiumClaim;
        }
        public Row[] rows;
        public TMP_Text headerLevel,headerProgress,axisProgress;
        public RectTransform axisBadge,axisValue,axisLine;
        public RectTransform headerFill;
        public Sprite actionIcon,rewardIcon;
        public UIRewardArt art;
        public GameObject unlockIcon;
        public Button unlockButton;
        public UIRewardPopup rewardPopup;
        private UIBackendClient client;
        private void OnEnable()
        {
            client=GetComponentInParent<UIBackendClient>();
            if(client){client.Changed+=Apply;if(client.State!=null)Apply(client.State);}
        }
        private void OnDisable(){if(client)client.Changed-=Apply;if(rewardPopup)rewardPopup.Close();}
        public void ClaimReward(int level,string track)
        {
            if(!client||client.Busy)return;
            client.Send("/v1/pass/"+level+"/claim","{\"track\":\""+track+"\"}",reply=>
            {
                if(reply.result.reward!=null)rewardPopup.Show(reply.result.reward);
            });
        }
        public void Apply(BackendState state)
        {
            if(state.rewards==null)return;
            if(headerLevel)headerLevel.text=state.player.pass_level.ToString();
            string progress=state.player.pass_level>=40?"已满级":$"{state.player.pass_xp%5} / 5";
            if(headerProgress)headerProgress.text=progress;
            if(headerFill){float fraction=state.player.pass_level>=40?1f:(state.player.pass_xp%5)/5f;headerFill.gameObject.SetActive(fraction>0);headerFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,125*fraction);}
            if(axisProgress)axisProgress.text=progress;
            // The native marker was authored after level 3. Move the two existing objects together.
            float completed=Mathf.Clamp(state.player.pass_level,0,40);
            if(axisBadge){axisBadge.gameObject.SetActive(completed>0&&completed<40);var pos=axisBadge.anchoredPosition;pos.y=-completed*99+43;axisBadge.anchoredPosition=pos;}
            if(axisValue){axisValue.gameObject.SetActive(completed>0&&completed<40);var pos=axisValue.anchoredPosition;pos.y=-completed*99+39;axisValue.anchoredPosition=pos;}
            if(axisLine){axisLine.gameObject.SetActive(completed>0&&completed<40);var pos=axisLine.anchoredPosition;pos.y=-completed*99+28;axisLine.anchoredPosition=pos;}
            foreach(var row in rows)
            {
                var config=state.pass_levels.First(x=>x.level==row.level);
                var free=state.rewards.First(x=>x.reward_id==config.free_reward_id);var paid=state.rewards.First(x=>x.reward_id==config.premium_reward_id);
                row.freeQuantity.text=art.Quantity(free);row.premiumQuantity.text=art.Quantity(paid);
                row.freeIcon.sprite=art.Icon(free);row.premiumIcon.sprite=art.Icon(paid);
                bool freeClaimed=state.pass_claims.Any(x=>x.level==row.level&&x.track=="FREE"),paidClaimed=state.pass_claims.Any(x=>x.level==row.level&&x.track=="PREMIUM");
                row.freeIcon.color=freeClaimed?new Color(1,1,1,.38f):Color.white;row.premiumIcon.color=paidClaimed?new Color(1,1,1,.38f):Color.white;
                bool complete=state.player.pass_xp>=config.required_xp;
                if(row.freeClaim)row.freeClaim.interactable=complete&&!freeClaimed;
                if(row.premiumClaim)row.premiumClaim.interactable=complete&&state.player.premium!=0&&!paidClaimed;
                if(row.freeMarker)row.freeMarker.SetActive(!freeClaimed);
                if(row.freeLockIcon)row.freeLockIcon.SetActive(!complete);
                if(row.premiumLock)
                {
                    row.premiumLock.SetActive(!paidClaimed);
                    var button=row.premiumLock.GetComponent<Button>();
                    if(button)button.interactable=state.player.premium==0;
                }
                if(row.premiumLockIcon)row.premiumLockIcon.SetActive(state.player.premium==0||!complete);
                if(row.diamond)row.diamond.color=complete?new Color32(255,213,0,255):new Color32(92,92,92,255);
                if(row.levelLabel)row.levelLabel.color=complete?Color.black:Color.white;
            }
            if(unlockIcon)unlockIcon.SetActive(state.player.premium==0);
            if(unlockButton)unlockButton.gameObject.SetActive(state.player.premium==0);
        }
    }
}

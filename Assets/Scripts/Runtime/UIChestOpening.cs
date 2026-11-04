using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ClashRoyaleUI
{
    public sealed class UIChestOpening : MonoBehaviour
    {
        public UIRewardArt art;
        public Image background,chest;
        public Image[] questionMarks;
        public Sprite questionSprite,spentSprite;
        public Button tap;
        public RectTransform rewardPanel;
        public Image[] rewardIcons;
        public TMP_Text[] rewardAmounts;
        public float turnDuration=.6f;
        private UIBackendClient client;
        private string boxId;
        private int rolls,star;
        private bool animating,finished;
        public int CompletedRolls=>rolls;
        public void Begin(ChestData box)
        {
            client=GetComponentInParent<UIBackendClient>();boxId=box.box_id;rolls=box.roll_count;star=Mathf.Max(1,box.current_star);
            animating=finished=false;rewardPanel.gameObject.SetActive(false);chest.gameObject.SetActive(true);
            chest.rectTransform.localScale=Vector3.one*Mathf.Pow(1.2f,rolls);chest.rectTransform.localRotation=Quaternion.identity;
            Paint();
        }
        private void Paint()
        {
            background.color=art.Color(star);chest.sprite=art.Chest(star);
            for(int i=0;i<questionMarks.Length;i++)
            {questionMarks[i].gameObject.SetActive(true);questionMarks[i].sprite=i<rolls?spentSprite:questionSprite;questionMarks[i].color=i<rolls?new Color32(55,63,65,255):UnityEngine.Color.white;questionMarks[i].rectTransform.sizeDelta=Vector2.one*(i<rolls?34:44);}
        }
        public void Tap()
        {
            if(animating||client==null||client.Busy)return;
            if(finished){GetComponentInParent<UIScreenRouter>().Show(ScreenId.ChestQueue);return;}
            StartCoroutine(Roll());
        }
        private IEnumerator Roll()
        {
            animating=true;BackendReply.Result result=null;
            client.Send("/v1/chests/"+boxId+"/open","{\"step\":"+(rolls+1)+"}",r=>result=r.result);
            float elapsed=0,fromScale=Mathf.Pow(1.2f,rolls);
            while(elapsed<turnDuration)
            {
                elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/turnDuration),e=1-Mathf.Pow(1-t,3);
                chest.rectTransform.localEulerAngles=new Vector3(0,360*e,0);
                chest.rectTransform.localScale=Vector3.one*fromScale*Mathf.Lerp(1,1.2f,e);
                yield return null;
            }
            chest.rectTransform.localRotation=Quaternion.identity;
            while(client.Busy)yield return null;
            if(result!=null)
            {
                star=result.final_star;rolls=result.roll_count;Paint();
                chest.rectTransform.localScale=Vector3.one*Mathf.Pow(1.2f,rolls);
                if(result.opened)
                {
                    yield return new WaitForSecondsRealtime(.22f);ShowRewards(result.reward);finished=true;
                    for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){float k=Mathf.Clamp01(t/.28f);rewardPanel.localScale=Vector3.one*Mathf.Lerp(.55f,1,1-Mathf.Pow(1-k,3));yield return null;}
                    rewardPanel.localScale=Vector3.one;
                }
            }
            else chest.rectTransform.localScale=Vector3.one*fromScale;
            animating=false;
        }
        private void ShowRewards(RewardData reward)
        {
            chest.gameObject.SetActive(false);foreach(var q in questionMarks)q.gameObject.SetActive(false);
            rewardPanel.gameObject.SetActive(true);int count=0;
            void Add(string kind,int amount){if(amount<=0)return;var icon=rewardIcons[count];icon.sprite=art.Resource(kind);icon.transform.parent.gameObject.SetActive(true);rewardAmounts[count].text="×"+amount;count++;}
            foreach(var icon in rewardIcons)icon.transform.parent.gameObject.SetActive(false);
            Add("gold",reward.gold);Add("common",reward.cards);Add("ap",reward.ap);Add(reward.resource,reward.amount);
            for(int i=0;i<count;i++){var rt=(RectTransform)rewardIcons[i].transform.parent;rt.anchoredPosition=new Vector2((i-(count-1)*.5f)*160,0);}
        }
    }
}

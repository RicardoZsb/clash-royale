using TMPro;
using UnityEngine;
namespace ClashRoyaleUI
{
    public sealed class UIChestDataView : MonoBehaviour
    {
        public bool reveal;
        public GameObject[] slots,panels;
        public TMP_Text[] stars;
        public UIChestQueueRow[] rows;
        private UIBackendClient client;
        private void OnEnable()
        {
            if(reveal)return;
            client=GetComponentInParent<UIBackendClient>();
            if(client){client.Changed+=Apply;if(client.State!=null)Apply(client.State);}
        }
        private void OnDisable(){if(client)client.Changed-=Apply;}
        public void ShowStar(int star){if(stars!=null&&stars.Length>0)stars[0].text=new string('★',Mathf.Clamp(star,1,5));}
        private void Apply(BackendState state)
        {
            if(rows!=null&&rows.Length>0)
            {
                for(int i=0;i<rows.Length;i++){bool visible=i<state.chests.Length;rows[i].gameObject.SetActive(visible);if(visible)rows[i].Bind(state.chests[i],i);}return;
            }
            for(int i=0;i<slots.Length;i++){bool visible=i<state.chests.Length;slots[i].SetActive(visible);if(visible)stars[i].text=new string('★',state.chests[i].initial_star);}
            if(panels.Length>0)panels[0].SetActive(state.chests.Length>0);
            if(panels.Length>1)panels[1].SetActive(state.chests.Length>2);
        }
    }
}

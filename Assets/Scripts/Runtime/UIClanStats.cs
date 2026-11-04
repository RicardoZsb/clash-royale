using System.Globalization;
using TMPro;
using UnityEngine;

namespace ClashRoyaleUI
{
    public sealed class UIClanStats : MonoBehaviour
    {
        public TMP_Text[] personalContribution,clanTokens,clanScore;
        private UIBackendClient client;
        private void OnEnable()
        {
            client=GetComponentInParent<UIBackendClient>();
            if(client){client.Changed+=Apply;if(client.State!=null)Apply(client.State);}
        }
        private void OnDisable(){if(client)client.Changed-=Apply;}
        public void Apply(BackendState state)
        {
            Set(personalContribution,state.player.season_contribution);
            Set(clanTokens,state.player.tokens);
            Set(clanScore,state.player.clan_score);
        }
        private static void Set(TMP_Text[] labels,float value)
        {
            string text=value.ToString("#,0.####",CultureInfo.InvariantCulture);
            foreach(var label in labels)label.text=text;
        }
    }
}

using UnityEngine;

namespace ClashRoyaleUI
{
    public sealed class UIPassRewardClaim : MonoBehaviour
    {
        public int level;
        public string track;
        public void Claim()=>GetComponentInParent<UIPassDataBinding>().ClaimReward(level,track);
    }
}

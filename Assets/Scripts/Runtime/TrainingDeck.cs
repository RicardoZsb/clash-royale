using System;
using UnityEngine;

namespace ClashRoyaleUI
{
    [Serializable]
    public sealed class TrainingCard
    {
        public string id, title;
        public int cost, count = 1;
        public float health, damage, speed, range, attackInterval = 1.1f, radius = 12, artSize = 58, splash, stun;
        public bool flying, hitsAir, buildingsOnly, summons, spell;
        public Sprite portrait;
        public RuntimeAnimatorController animator;
    }
    [CreateAssetMenu(menuName = "Clash Royale/Training Deck")]
    public sealed class TrainingDeck : ScriptableObject
    {
        public TrainingCard[] cards;
        public TrainingCard skeleton;
        public RuntimeAnimatorController princessAnimator, kingAnimator;
        public Sprite princessPortrait, kingPortrait;
        public float princessHealth = 1450, kingHealth = 2400;
        public float regulationSeconds = 180, overtimeSeconds = 60, elixirSeconds = 2.8f;
    }
}

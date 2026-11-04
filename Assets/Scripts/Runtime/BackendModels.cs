using System;
namespace ClashRoyaleUI
{
    [Serializable] public sealed class PlayerData
    {
        public string player_id, season_id, clan_id, forced_instance_id;
        public int activation_order;
        public int normal_ap, reserve_ap, normal_cap, reserve_cap, reserved_ap, available_ap;
        public int gold, gems, cards, pass_xp, pass_level, premium, pending_chests, queue_capacity, reserved_slots, accepted_count, active_count;
        public float tokens, season_contribution, clan_score;
    }
    [Serializable] public sealed class TaskData
    {
        public string task_id, title, brief, description, instance_id, status, condition_json, metric;
        public int tier, target_count, progress, action_cost, base_contribution, max_attempts;
        public bool help_requested, forced;
    }
    [Serializable] public sealed class ChestData { public string box_id, chest_id; public int initial_star,current_star,roll_count; }
    [Serializable] public sealed class PassLevelData { public int level, required_xp; public string free_reward_id, premium_reward_id; }
    [Serializable] public sealed class PassClaimData { public int level; public string track; }
    [Serializable] public sealed class HelpData { public string help_id, player_id, message; }
    [Serializable] public sealed class RewardData { public string reward_id,resource; public int gold, cards, ap,amount,chest_star; }
    [Serializable] public sealed class BackendState
    {
        public bool ok;
        public string bundle_id, week_id, battle_id, error;
        public PlayerData player;
        public TaskData[] tasks;
        public ChestData[] chests;
        public PassLevelData[] pass_levels;
        public PassClaimData[] pass_claims;
        public HelpData[] clan_help;
        public RewardData[] rewards;
    }
    [Serializable] public sealed class BackendReply
    {
        public bool ok; public string error; public Result result;
        [Serializable] public sealed class Result { public string message, instance_id, battle_id, box_id, reward_id, outcome; public int final_star, amount, completed_count,roll_count,premium; public bool opened,upgraded,already_claimed; public RewardData reward; }
    }
}

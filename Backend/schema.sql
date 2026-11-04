PRAGMA foreign_keys=ON;
CREATE TABLE IF NOT EXISTS schema_version(version INTEGER PRIMARY KEY);
INSERT OR IGNORE INTO schema_version VALUES(1);
CREATE TABLE IF NOT EXISTS config_bundles(bundle_id TEXT PRIMARY KEY,sha256 TEXT NOT NULL UNIQUE,imported_at INTEGER NOT NULL,source_path TEXT NOT NULL,payload TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS service_meta(key TEXT PRIMARY KEY,value TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS task_templates(bundle_id TEXT NOT NULL REFERENCES config_bundles,task_id TEXT NOT NULL,tier INTEGER NOT NULL CHECK(tier BETWEEN 1 AND 4),title TEXT NOT NULL,brief TEXT NOT NULL,description TEXT NOT NULL,payload TEXT NOT NULL,PRIMARY KEY(bundle_id,task_id));
CREATE TABLE IF NOT EXISTS players(player_id TEXT PRIMARY KEY,season_id TEXT NOT NULL,normal_ap INTEGER NOT NULL CHECK(normal_ap>=0),reserve_ap INTEGER NOT NULL CHECK(reserve_ap>=0),gold INTEGER NOT NULL CHECK(gold>=0),cards INTEGER NOT NULL CHECK(cards>=0),gems INTEGER NOT NULL DEFAULT 0,pass_xp INTEGER NOT NULL DEFAULT 0,premium INTEGER NOT NULL DEFAULT 0,tokens REAL NOT NULL DEFAULT 0 CHECK(tokens>=0),season_contribution REAL NOT NULL DEFAULT 0,clan_id TEXT,joined_at INTEGER,tower_level INTEGER NOT NULL DEFAULT 10,audience TEXT NOT NULL DEFAULT 'NORMAL',recovery_at INTEGER NOT NULL,extra_ap_used INTEGER NOT NULL DEFAULT 0,shop_ap_bought INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS task_instances(instance_id TEXT PRIMARY KEY,player_id TEXT NOT NULL REFERENCES players,task_id TEXT NOT NULL,bundle_id TEXT NOT NULL REFERENCES config_bundles,season_id TEXT NOT NULL,week_id TEXT NOT NULL,status TEXT NOT NULL CHECK(status IN ('accepted','active','completed','cancelled','expired')),progress INTEGER NOT NULL DEFAULT 0,attempts INTEGER NOT NULL DEFAULT 0,deck_keys TEXT NOT NULL DEFAULT '[]',accepted_at INTEGER NOT NULL,completed_at INTEGER,config_snapshot TEXT NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS one_live_task ON task_instances(player_id,task_id,week_id) WHERE status IN ('accepted','active','completed');
CREATE TABLE IF NOT EXISTS task_activation_preferences(player_id TEXT PRIMARY KEY REFERENCES players,order_rule INTEGER NOT NULL DEFAULT 0 CHECK(order_rule IN (0,1,2)),forced_instance_id TEXT REFERENCES task_instances);
CREATE TABLE IF NOT EXISTS battles(battle_id TEXT PRIMARY KEY,player_id TEXT NOT NULL REFERENCES players,bundle_id TEXT NOT NULL REFERENCES config_bundles,season_id TEXT NOT NULL,week_id TEXT NOT NULL,status TEXT NOT NULL CHECK(status IN ('running','settled')),started_at INTEGER NOT NULL,settled_at INTEGER,facts TEXT,result TEXT);
CREATE UNIQUE INDEX IF NOT EXISTS one_running_battle ON battles(player_id) WHERE status='running';
CREATE TABLE IF NOT EXISTS training_sessions(battle_id TEXT PRIMARY KEY REFERENCES battles,player_id TEXT NOT NULL REFERENCES players,outcome TEXT,completed_instances TEXT,elapsed_seconds INTEGER);
CREATE TABLE IF NOT EXISTS activations(activation_id TEXT PRIMARY KEY,instance_id TEXT NOT NULL REFERENCES task_instances,player_id TEXT NOT NULL REFERENCES players,battle_id TEXT REFERENCES battles,ap_reserved INTEGER NOT NULL CHECK(ap_reserved>0),slots_reserved INTEGER NOT NULL DEFAULT 1,state TEXT NOT NULL CHECK(state IN ('reserved','battle','released','committed')),created_at INTEGER NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS one_active_reservation ON activations(instance_id) WHERE state IN ('reserved','battle');
CREATE TABLE IF NOT EXISTS settlements(settlement_id TEXT PRIMARY KEY,instance_id TEXT NOT NULL UNIQUE REFERENCES task_instances,activation_id TEXT NOT NULL UNIQUE REFERENCES activations,battle_id TEXT NOT NULL REFERENCES battles,player_id TEXT NOT NULL REFERENCES players,ap_delta INTEGER NOT NULL,pass_xp_delta INTEGER NOT NULL,first_gold INTEGER NOT NULL,created_at INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS first_clears(player_id TEXT NOT NULL REFERENCES players,season_id TEXT NOT NULL,task_id TEXT NOT NULL,settlement_id TEXT NOT NULL UNIQUE REFERENCES settlements,PRIMARY KEY(player_id,season_id,task_id));
CREATE TABLE IF NOT EXISTS chests(box_id TEXT PRIMARY KEY,player_id TEXT NOT NULL REFERENCES players,bundle_id TEXT NOT NULL REFERENCES config_bundles,chest_id TEXT NOT NULL,source_settlement_id TEXT UNIQUE REFERENCES settlements,status TEXT NOT NULL DEFAULT 'pending' CHECK(status IN ('pending','opened')),initial_star INTEGER NOT NULL,final_star INTEGER,rolls TEXT,reward_id TEXT,created_at INTEGER NOT NULL,opened_at INTEGER);
CREATE TABLE IF NOT EXISTS pass_claims(player_id TEXT NOT NULL REFERENCES players,season_id TEXT NOT NULL,level INTEGER NOT NULL,track TEXT NOT NULL CHECK(track IN ('FREE','PREMIUM')),reward_id TEXT NOT NULL,claimed_at INTEGER NOT NULL,PRIMARY KEY(player_id,season_id,level,track));
CREATE TABLE IF NOT EXISTS clan_members(player_id TEXT NOT NULL,clan_id TEXT NOT NULL,season_id TEXT NOT NULL,contribution REAL NOT NULL DEFAULT 0,completed_tasks INTEGER NOT NULL DEFAULT 0,active INTEGER NOT NULL DEFAULT 1,PRIMARY KEY(player_id,clan_id,season_id));
CREATE TABLE IF NOT EXISTS contribution_events(event_id TEXT PRIMARY KEY,settlement_id TEXT NOT NULL UNIQUE REFERENCES settlements,player_id TEXT NOT NULL,clan_id TEXT NOT NULL,season_id TEXT NOT NULL,base_amount REAL NOT NULL,actual_amount REAL NOT NULL,token_amount REAL NOT NULL,created_at INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS task_help_requests(help_id TEXT PRIMARY KEY,instance_id TEXT NOT NULL UNIQUE REFERENCES task_instances,player_id TEXT NOT NULL REFERENCES players,clan_id TEXT NOT NULL,message TEXT NOT NULL,created_at INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS resource_ledger(ledger_id INTEGER PRIMARY KEY AUTOINCREMENT,player_id TEXT NOT NULL REFERENCES players,resource TEXT NOT NULL,amount REAL NOT NULL,source_type TEXT NOT NULL,source_id TEXT NOT NULL,balance REAL NOT NULL,created_at INTEGER NOT NULL,UNIQUE(player_id,resource,source_type,source_id));
CREATE TABLE IF NOT EXISTS request_results(player_id TEXT NOT NULL,request_id TEXT NOT NULL,request_hash TEXT NOT NULL,result TEXT NOT NULL,created_at INTEGER NOT NULL,PRIMARY KEY(player_id,request_id));
CREATE TABLE IF NOT EXISTS events(event_id INTEGER PRIMARY KEY AUTOINCREMENT,player_id TEXT,kind TEXT NOT NULL,payload TEXT NOT NULL,created_at INTEGER NOT NULL);
CREATE INDEX IF NOT EXISTS player_tasks ON task_instances(player_id,status);
CREATE INDEX IF NOT EXISTS player_chests ON chests(player_id,status);
CREATE INDEX IF NOT EXISTS player_activations ON activations(player_id,state);
CREATE TABLE IF NOT EXISTS player_items (
 player_id TEXT NOT NULL REFERENCES players(player_id),
 resource TEXT NOT NULL,
 amount INTEGER NOT NULL DEFAULT 0 CHECK(amount>=0),
 PRIMARY KEY(player_id,resource)
);
CREATE TABLE IF NOT EXISTS chest_openings (
 box_id TEXT PRIMARY KEY REFERENCES chests(box_id),
 current_star INTEGER NOT NULL,
 roll_count INTEGER NOT NULL DEFAULT 0 CHECK(roll_count BETWEEN 0 AND 4),
 rolls TEXT NOT NULL DEFAULT '[]'
);

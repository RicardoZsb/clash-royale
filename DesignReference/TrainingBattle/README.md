# 训练营对局

打开 `Assets/Scenes/ClashRoyaleUI.unity`，Play 后点击主页的交叉剑匹配按钮。

## 可编辑资产

| 内容 | 位置 |
|---|---|
| 完整 uGUI 对局层级 | `Assets/Prefabs/Screens/TrainingBattle.prefab` |
| 单位、投射物组件 | `Assets/Prefabs/Components/TrainingUnit.prefab`、`TrainingProjectile.prefab` |
| 默认卡组和战斗数值 | `Assets/Tables/TrainingDeck.asset` |
| PNG 帧图和原生 Sprite 切片 | `Assets/Art/TrainingBattle` |
| Animator 和 AnimationClip | `Assets/Animations/TrainingBattle` |
| 战斗、出牌、目标选择、胜负 | `Assets/Scripts/Runtime/TrainingBattleController.cs` |
| 单位行为和动画事件 | `Assets/Scripts/Runtime/TrainingActor.cs` |
| 法术、投射物和命中事件 | `Assets/Scripts/Runtime/TrainingProjectile.cs` |
| 手牌点击和拖动 | `Assets/Scripts/Runtime/TrainingHandCard.cs` |
| 匹配入口和页面跳转 | `Assets/Scripts/Runtime/UIBackendBindings.cs`、`UIScreenRouter.cs` |

卡组为骑士、弓箭手、巨人、飞龙宝宝、女巫、火球、万箭齐发、电击法术。女巫召唤骷髅。双方各有两座公主塔、一座国王塔。国王塔在受伤或己方公主塔被摧毁后参战。

开局 5 点圣水，上限 10 点；基础每 2.8 秒恢复 1 点，最后一分钟与加时双倍回复。四张手牌轮转，左侧显示下一张。单位从己方半场部署，地面单位经过两座桥，飞龙直接飞行。法术可在全场释放。

常规时间 180 秒，皇冠领先获胜；平分进入 60 秒突然死亡加时。国王塔被毁立即结束。加时结束比较双方存活塔的最低血量，仍相同则平局。

部队 Animator 有 Spawn、Idle、Move、Attack、Summon、Die 六个状态；法术有 Cast、Travel、Impact 三个状态。动画通过 Image.m_Sprite 帧曲线播放，Strike、Summon、Impact 等 AnimationEvent 在动作帧触发伤害和召唤。界面、64 个单位槽、32 个投射物槽均已保存在预制体内，运行时不生成 UI 层级。

`BuildTrainingBattle.cs` 是已执行的一次性编辑器拼装工具；后续直接修改预制体和数值资产即可。切片脚本 `SliceAtlases.cs` 使用 Unity 原生 Sprite Data Provider，帧名和 GUID 保持稳定。

美术由内置 image_gen 工具生成，提示词与源文件记录在同目录 `art-manifest.json`；项目只引用已复制进 Assets 的资产。测试存档位于 `DesignReference/Validation/TrainingBattle/.local`，与玩家正式存档分开。

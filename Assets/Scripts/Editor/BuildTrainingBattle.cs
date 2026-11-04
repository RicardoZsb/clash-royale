using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleUI.Editor
{
    // One-time authoring command. Runtime only reuses the saved objects and assets.
    public static class BuildTrainingBattle
    {
        const string AnimationFolder = "Assets/Animations/TrainingBattle";
        const string PrefabPath = "Assets/Prefabs/Screens/TrainingBattle.prefab";
        static TMP_FontAsset font;
        static Sprite rounded, circle;
        static readonly Color ink = Hex("34323B"), cream = Hex("F1E7CD");
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var c); return c; }

        [MenuItem("Clash Royale/创建训练营对局（一次）")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("先停止运行。");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)) throw new InvalidOperationException("对局已创建，请编辑现有预制体。");
            Directory.CreateDirectory(AnimationFolder);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansSC SDF.asset");
            rounded=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Panel_14_14_14_14_0_False.png");
            circle=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var deck=CreateDeck();
            var router=UnityEngine.Object.FindFirstObjectByType<UIScreenRouter>();
            var root=Rect("TrainingBattle · 训练营",router.transform,201,-437,402,874);
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            var board=Rect("Board · 402 × 874",root,0,0,402,874);
            board.anchorMin=board.anchorMax=new Vector2(.5f,.5f);
            root.gameObject.AddComponent<TrainingBattleLayout>().Board=board;
            var backdrop=root.gameObject.AddComponent<Image>();backdrop.color=Hex("AAA7A5");backdrop.raycastTarget=false;
            var battle=root.gameObject.AddComponent<TrainingBattleController>();battle.deck=deck;
            Image("Backdrop",board,0,0,402,874,Hex("AAA7A5"),false);
            Panel("ArenaBorder",board,0,63,394,638,Hex("B5B0CF"));
            var arena=Rect("Arena",board,0,63,382,620);battle.arena=arena;
            Image("Grass",arena,0,0,382,620,Hex("CAD6AC"),false);
            for(int y=0;y<14;y++)for(int x=0;x<8;x++)
                Image("Tile "+x+"-"+y,arena,(x-3.5f)*47.75f,(y-6.5f)*44.286f,47.75f,44.286f,(x+y)%2==0?Hex("C6D3A6"):Hex("D3DDB7"),false);
            for(int side=-1;side<=1;side+=2)for(int lane=-1;lane<=1;lane+=2)
                Image("Lane",arena,lane*104,side*156,58,264,Hex("DDDEB6"),false);
            Image("RiverEdge",arena,0,0,382,58,ink,false);
            Image("River",arena,0,0,382,51,Hex("93C9D5"),false);
            for(int i=0;i<9;i++)Image("WaterLine",arena,-169+i*42,i%2==0?12:-11,22,2,Hex("C6E7E6"),false);
            foreach(int side in new[]{-1,1})
            {
                Panel("Bridge",arena,side*104,0,59,68,Hex("EAC98B"),3);
                for(int j=-2;j<=2;j++)Image("Plank",arena,side*104,j*11,51,1.5f,Hex("8C7963"),false);
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)Panel("BridgePost",arena,side*104+x*27,y*31,9,11,Hex("F0D38B"),1);
            }
            var surface=Image("PlacementSurface",arena,0,0,382,620,Color.clear,false);surface.raycastTarget=true;
            surface.gameObject.AddComponent<TrainingBattlefield>().battle=battle;
            battle.enemyPlacementShade=Image("EnemyTerritory",arena,0,166,378,288,new Color(1,.25f,.25f,.10f),false);battle.enemyPlacementShade.gameObject.SetActive(false);
            var towerLayer=Rect("Towers",arena,0,0,382,620);
            battle.towers=new TrainingActor[6];
            for(int side=0;side<2;side++)for(int i=0;i<3;i++)
            {
                var tower=Actor((side==0?"Blue ":"Red ")+(i==2?"King":i==0?"Left Princess":"Right Princess"),towerLayer,true);
                tower.team=side;tower.king=i==2;tower.tower=true;
                tower.rect.anchoredPosition=new Vector2(i==2?0:i==0?-104:104,(side==0?-1:1)*(i==2?250:174));
                tower.art.sprite=i==2?deck.kingPortrait:deck.princessPortrait;tower.artRect.sizeDelta=Vector2.one*(i==2?85:73);
                tower.animator.runtimeAnimatorController=i==2?deck.kingAnimator:deck.princessAnimator;
                tower.healthText.text=i==2?"2400":"1450";tower.healthFill.color=tower.teamRing.color=side==0?Hex("58A5D8"):Hex("DF7D7C");
                battle.towers[side*3+i]=tower;
            }
            var pool=Rect("Units · 编辑器预置池",arena,0,0,382,620);
            var unit=Actor("Unit",pool,false);
            var unitPrefab=PrefabUtility.SaveAsPrefabAssetAndConnect(unit.gameObject,"Assets/Prefabs/Components/TrainingUnit.prefab",InteractionMode.AutomatedAction);
            battle.units=new TrainingActor[64];battle.units[0]=unit;
            for(int i=1;i<battle.units.Length;i++)battle.units[i]=((GameObject)PrefabUtility.InstantiatePrefab(unitPrefab,pool)).GetComponent<TrainingActor>();
            for(int i=0;i<battle.units.Length;i++){battle.units[i].name="Unit "+i.ToString("00");battle.units[i].gameObject.SetActive(false);}
            var effects=Rect("Projectiles",arena,0,0,382,620);
            var effect=Rect("Projectile",effects,0,0,32,32).gameObject.AddComponent<TrainingProjectile>();
            effect.rect=(RectTransform)effect.transform;effect.art=Image("Art",effect.transform,0,0,32,32,Color.white,false);effect.art.preserveAspect=true;effect.artRect=effect.art.rectTransform;
            effect.animator=effect.gameObject.AddComponent<Animator>();effect.animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var effectPrefab=PrefabUtility.SaveAsPrefabAssetAndConnect(effect.gameObject,"Assets/Prefabs/Components/TrainingProjectile.prefab",InteractionMode.AutomatedAction);
            battle.projectiles=new TrainingProjectile[32];battle.projectiles[0]=effect;
            for(int i=1;i<battle.projectiles.Length;i++)battle.projectiles[i]=((GameObject)PrefabUtility.InstantiatePrefab(effectPrefab,effects)).GetComponent<TrainingProjectile>();
            for(int i=0;i<battle.projectiles.Length;i++){battle.projectiles[i].name="Projectile "+i.ToString("00");battle.projectiles[i].gameObject.SetActive(false);}
            battle.ghostRect=Rect("PlacementPreview",arena,0,0,64,64);
            battle.ghostRing=Image("Radius",battle.ghostRect,0,0,64,64,new Color(.4f,.8f,.7f,.5f));battle.ghostRing.sprite=circle;Stretch(battle.ghostRing.rectTransform);
            battle.ghostArt=Image("Card",battle.ghostRect,0,0,54,54,new Color(1,1,1,.65f),false);battle.ghostArt.preserveAspect=true;battle.ghostRect.gameObject.SetActive(false);

            Panel("Timer",board,0,409,84,39,cream);
            battle.timerLabel=Text("Time",board,0,410,80,36,"3:00",24);
            Text("Opponent",board,-119,414,130,26,"训练营",18,TextAlignmentOptions.MidlineLeft);
            Text("Player",board,-127,-266,120,22,"Matfy",15,TextAlignmentOptions.MidlineLeft);
            var crown=FindCrown();
            var red=Image("EnemyCrown",board,141,412,29,27,Color.white,false);red.sprite=crown;red.preserveAspect=true;
            battle.enemyCrownsLabel=Text("EnemyCrowns",board,176,412,30,30,"0",23);
            var blue=Image("PlayerCrown",board,145,-266,24,23,Color.white,false);blue.sprite=crown;blue.preserveAspect=true;
            battle.playerCrownsLabel=Text("PlayerCrowns",board,176,-266,26,26,"0",20);

            Panel("HandTray",board,0,-361,398,144,Hex("C4BCB5"));
            Panel("NextCardFrame",board,-170,-349,43,62,cream,2);
            battle.nextPortrait=Image("NextCard",board,-170,-348,40,57,Color.white,false);battle.nextPortrait.preserveAspect=true;
            battle.handViews=new TrainingHandCard[4];
            for(int i=0;i<4;i++)
            {
                var cardRoot=Rect("Hand "+(i+1),board,-103+i*79,-348,71,105);
                var card=cardRoot.gameObject.AddComponent<TrainingHandCard>();card.battle=battle;card.slot=i;
                var border=Panel("Frame",cardRoot,0,0,71,105,cream,2);
                card.frame=border.transform.Find("Fill").GetComponent<Image>();
                card.button=cardRoot.gameObject.AddComponent<Button>();card.button.targetGraphic=card.frame;
                var hit=cardRoot.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
                card.portrait=Image("Portrait",cardRoot,0,6,67,78,Color.white,false);card.portrait.preserveAspect=true;
                Image("Nameplate",cardRoot,0,-39,67,23,Hex("DDD2B7"));
                card.title=Text("Name",cardRoot,0,-39,69,23,"",12);
                var cost=Image("Elixir",cardRoot,-28,43,29,33,ink);cost.sprite=circle;
                var jewel=Image("Jewel",cost.transform,0,0,25,29,Hex("B88ED6"));jewel.sprite=circle;
                card.cost=Text("Cost",cost.transform,0,0,27,30,"",20);card.cost.color=Color.white;card.cost.fontStyle=FontStyles.Bold;
                card.shade=Image("Unavailable",cardRoot,0,0,71,105,new Color(.3f,.28f,.32f,.32f));
                UnityEventTools.AddPersistentListener(card.button.onClick,card.Select);
                card.Display(deck.cards[new[]{0,1,5,2}[i]],true,false);battle.handViews[i]=card;
            }
            Panel("ElixirTrack",board,20,-417,342,22,Hex("6B5378"),2);
            battle.elixirFill=Image("ElixirFill",board,20,-417,338,18,Hex("BD86D2"));battle.elixirFill.type=UnityEngine.UI.Image.Type.Filled;battle.elixirFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;battle.elixirFill.fillAmount=.5f;
            for(int i=1;i<10;i++)Image("ElixirTick",board,-149+i*33.8f,-417,1,18,Hex("866395"),false);
            var elixir=Image("ElixirOrb",board,-170,-413,32,36,ink);elixir.sprite=circle;
            var inner=Image("Inner",elixir.transform,0,0,28,32,Hex("BC86D4"));inner.sprite=circle;
            battle.elixirLabel=Text("ElixirValue",elixir.transform,0,0,30,34,"5",22);battle.elixirLabel.color=Color.white;

            var result=Image("Result",board,0,0,402,874,new Color(.18f,.17f,.22f,.63f),false);result.raycastTarget=true;battle.resultPanel=result.gameObject;
            Panel("ResultPanel",result.transform,0,0,302,280,cream,3);
            battle.resultTitle=Text("Title",result.transform,0,88,260,54,"胜利",34);battle.resultTitle.fontStyle=FontStyles.Bold;
            var rewardCrown=Image("Crown",result.transform,0,34,44,38,Color.white,false);rewardCrown.sprite=crown;rewardCrown.preserveAspect=true;
            battle.resultScore=Text("Score",result.transform,0,-10,220,42,"1 : 0",31);
            battle.resultRewards=Text("Rewards",result.transform,0,-54,260,30,"",18);
            var continueGraphic=Panel("Continue",result.transform,0,-101,184,43,Hex("F8D252"),2);
            continueGraphic.raycastTarget=true;battle.continueButton=continueGraphic.gameObject.AddComponent<Button>();battle.continueButton.targetGraphic=continueGraphic;
            battle.continueLabel=Text("Label",continueGraphic.transform,0,0,175,40,"继续",20);
            UnityEventTools.AddPersistentListener(battle.continueButton.onClick,battle.Continue);result.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject,PrefabPath,InteractionMode.AutomatedAction);
            router.Screens=router.Screens.Concat(new[]{new UIScreenRouter.ScreenEntry{id=ScreenId.TrainingBattle,view=root.gameObject}}).ToArray();
            router.Show(ScreenId.BattleHome);EditorUtility.SetDirty(router);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(router.gameObject.scene);
        }

        static TrainingDeck CreateDeck()
        {
            var deck=ScriptableObject.CreateInstance<TrainingDeck>();
            var knight=Troop("knight","骑士",3,480,70,36,22,1,58);
            var archer=Troop("archer","弓箭手",3,240,48,38,115,1.05f,56);archer.count=2;archer.hitsAir=true;
            var giant=Troop("giant","巨人",5,1350,105,24,30,1.6f,78);giant.buildingsOnly=true;giant.radius=19;
            var dragon=Troop("dragon","飞龙宝宝",4,650,65,34,82,1.3f,70);dragon.flying=true;dragon.hitsAir=true;dragon.splash=45;
            var witch=Troop("witch","女巫",5,400,45,30,100,1.3f,66);witch.hitsAir=true;witch.summons=true;witch.splash=22;
            deck.cards=new[]{knight,archer,giant,dragon,witch,Spell("fireball","火球",4,240,60,120,0),Spell("arrows","万箭齐发",3,110,75,150,2),Spell("zap","电击法术",2,80,60,120,4)};
            deck.cards[7].stun=.65f;
            deck.skeleton=Troop("skeleton","骷髅",0,85,28,44,18,1,42);deck.skeleton.radius=8;
            var towers=Frames("towers");deck.princessPortrait=towers[0];deck.kingPortrait=towers[6];
            deck.princessAnimator=ActorController("princess",towers,true,0);deck.kingAnimator=ActorController("king",towers,true,1);
            AssetDatabase.CreateAsset(deck,"Assets/Tables/TrainingDeck.asset");return deck;
        }
        static TrainingCard Troop(string id,string title,int cost,float hp,float damage,float speed,float range,float interval,float size)
        {
            var frames=Frames(id);return new TrainingCard{id=id,title=title,cost=cost,health=hp,damage=damage,speed=speed,range=range,attackInterval=interval,artSize=size,portrait=frames[6],animator=ActorController(id,frames,false,0)};
        }
        static TrainingCard Spell(string id,string title,int cost,float damage,float radius,float size,int row)
        {
            var frames=Frames("effects");var controller=AnimatorController.CreateAnimatorControllerAtPath(AnimationFolder+"/"+id+".controller");controller.AddParameter("Impact",AnimatorControllerParameterType.Trigger);
            var sm=controller.layers[0].stateMachine;
            var cast=State(sm,"Cast",Clip(id,"Cast",frames.Skip(row*6).Take(3).ToArray(),10,false),0,0);
            var travel=State(sm,"Travel",Clip(id,"Travel",frames.Skip(row*6+3).Take(3).ToArray(),10,true),220,0);
            var impact=State(sm,"Impact",Clip(id,"Impact",frames.Skip((row+1)*6).Take(6).ToArray(),10,false,new AnimationEvent{time=.08f,functionName="Impact"},new AnimationEvent{time=.58f,functionName="EffectFinished"}),220,110);
            sm.defaultState=cast;Exit(cast,travel);Trigger(sm,impact,"Impact");
            return new TrainingCard{id=id,title=title,cost=cost,damage=damage,splash=radius,artSize=size,spell=true,portrait=frames[row*6+2],animator=controller};
        }
        static Sprite[] Frames(string id)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Art/TrainingBattle/"+id+".png").OfType<Sprite>().OrderBy(x=>x.name).ToArray();
        static RuntimeAnimatorController ActorController(string id,Sprite[] f,bool tower,int towerRow)
        {
            var controller=AnimatorController.CreateAnimatorControllerAtPath(AnimationFolder+"/"+id+".controller");
            controller.AddParameter("Moving",AnimatorControllerParameterType.Bool);
            foreach(var p in new[]{"Attack","Summon","Die"})controller.AddParameter(p,AnimatorControllerParameterType.Trigger);
            var sm=controller.layers[0].stateMachine;bool skeleton=id=="skeleton";
            Func<int,Sprite[]> row=r=>f.Skip(r*6).Take(6).ToArray();
            var idleFrames=tower?new[]{f[towerRow*6]}:row(1);
            var spawn=State(sm,"Spawn",Clip(id,"Spawn",tower?idleFrames:row(0),10,false),0,0);
            var idle=State(sm,"Idle",Clip(id,"Idle",idleFrames,6,true),220,0);
            var move=State(sm,"Move",Clip(id,"Move",tower?idleFrames:row(skeleton?1:2),10,true),440,0);
            var attackFrames=tower?row(towerRow):row(skeleton?2:3);
            if(id=="witch")attackFrames[4]=attackFrames[3];
            var attack=State(sm,"Attack",Clip(id,"Attack",attackFrames,10,false,new AnimationEvent{time=.3f,functionName="Strike"}),220,120);
            var summon=State(sm,"Summon",Clip(id,"Summon",tower?idleFrames:row(skeleton?3:4),10,false,new AnimationEvent{time=.3f,functionName="Summon"}),440,120);
            var die=State(sm,"Die",Clip(id,"Die",tower?row(2):row(skeleton?4:5),10,false,new AnimationEvent{time=.58f,functionName="DeathFinished"}),220,240);
            sm.defaultState=spawn;Exit(spawn,idle);Exit(attack,idle);Exit(summon,idle);
            var m=idle.AddTransition(move);m.duration=0;m.AddCondition(AnimatorConditionMode.If,0,"Moving");
            m=move.AddTransition(idle);m.duration=0;m.AddCondition(AnimatorConditionMode.IfNot,0,"Moving");
            Trigger(sm,die,"Die");Trigger(sm,attack,"Attack");Trigger(sm,summon,"Summon");return controller;
        }
        static AnimatorState State(AnimatorStateMachine sm,string name,AnimationClip clip,int x,int y){var s=sm.AddState(name,new Vector3(x,y));s.motion=clip;s.writeDefaultValues=false;return s;}
        static void Exit(AnimatorState from,AnimatorState to){var t=from.AddTransition(to);t.hasExitTime=true;t.exitTime=1;t.duration=0;}
        static void Trigger(AnimatorStateMachine sm,AnimatorState to,string trigger){var t=sm.AddAnyStateTransition(to);t.duration=0;t.canTransitionToSelf=false;t.AddCondition(AnimatorConditionMode.If,0,trigger);}
        static AnimationClip Clip(string id,string name,Sprite[] frames,float fps,bool loop,params AnimationEvent[] events)
        {
            var clip=new AnimationClip{name=id+"_"+name,frameRate=fps};
            var keys=frames.Select((f,i)=>new ObjectReferenceKeyframe{time=i/fps,value=f}).ToList();
            keys.Add(new ObjectReferenceKeyframe{time=frames.Length/fps,value=frames.Last()});
            AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="Art",type=typeof(Image),propertyName="m_Sprite"},keys.ToArray());
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            AnimationUtility.SetAnimationEvents(clip,events);AssetDatabase.CreateAsset(clip,AnimationFolder+"/"+id+"_"+name+".anim");return clip;
        }
        static TrainingActor Actor(string name,Transform parent,bool tower)
        {
            var r=Rect(name,parent,0,0,64,64);var a=r.gameObject.AddComponent<TrainingActor>();a.rect=r;
            a.teamRing=Image("TeamRing",r,0,-19,tower?65:38,tower?19:11,Hex("58A5D8"));a.teamRing.sprite=circle;
            a.art=Image("Art",r,0,6,58,58,Color.white,false);a.art.preserveAspect=true;a.artRect=a.art.rectTransform;
            var hp=Panel("Health",r,0,tower?-36:35,tower?62:33,tower?12:5,ink,0);
            a.healthFill=Image("Fill",hp.transform,0,0,tower?58:31,tower?8:3,Hex("58A5D8"),false);a.healthFill.type=UnityEngine.UI.Image.Type.Filled;a.healthFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            if(tower){a.healthText=Text("HP",hp.transform,0,0,60,13,"",10);a.healthText.color=Color.white;}
            a.animator=r.gameObject.AddComponent<Animator>();a.animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;return a;
        }
        static Sprite FindCrown()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Generated/Crop_36.asset");
        }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static Image Image(string name,Transform parent,float x,float y,float w,float h,Color color,bool round=true)
        {
            var r=Rect(name,parent,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;
            if(round){i.sprite=rounded;i.type=UnityEngine.UI.Image.Type.Sliced;}return i;
        }
        static Image Panel(string name,Transform parent,float x,float y,float w,float h,Color color,float border=2)
        {
            var outer=Image(name,parent,x,y,w,h,border>0?ink:color);
            if(border<=0)return outer;
            Image("Fill",outer.transform,0,0,w-border*2,h-border*2,color);return outer;
        }
        static TMP_Text Text(string name,Transform parent,float x,float y,float w,float h,string value,int size,TextAlignmentOptions alignment=TextAlignmentOptions.Center)
        {
            var t=Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.color=ink;t.text=value;
            t.alignment=alignment;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Overflow;return t;
        }
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    }
}

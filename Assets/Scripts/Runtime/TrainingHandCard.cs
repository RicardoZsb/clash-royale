using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClashRoyaleUI
{
    public sealed class TrainingHandCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TrainingBattleController battle;
        public int slot;
        public Image portrait, frame, shade;
        public TMP_Text cost, title;
        public Button button;
        public void Select()=>battle.SelectCard(slot);
        public void OnBeginDrag(PointerEventData e){battle.SelectCard(slot,false);battle.PreviewPlacement(e.position,e.pressEventCamera);}
        public void OnDrag(PointerEventData e)=>battle.PreviewPlacement(e.position,e.pressEventCamera);
        public void OnEndDrag(PointerEventData e)=>battle.PlayAtScreen(e.position,e.pressEventCamera);
        public void Display(TrainingCard card,bool affordable,bool selected)
        {
            portrait.sprite=card.portrait;cost.text=card.cost.ToString();title.text=card.title;
            shade.gameObject.SetActive(!affordable);frame.color=selected?new Color32(255,218,92,255):new Color32(245,235,211,255);
        }
    }
}

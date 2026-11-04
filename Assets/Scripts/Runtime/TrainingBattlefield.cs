using UnityEngine;
using UnityEngine.EventSystems;

namespace ClashRoyaleUI
{
    public sealed class TrainingBattlefield : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
    {
        public TrainingBattleController battle;
        public void OnPointerClick(PointerEventData e)=>battle.PlayAtScreen(e.position,e.pressEventCamera);
        public void OnPointerMove(PointerEventData e)=>battle.PreviewPlacement(e.position,e.pressEventCamera);
    }
}

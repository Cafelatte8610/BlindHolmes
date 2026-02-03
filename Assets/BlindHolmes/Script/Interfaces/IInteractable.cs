using UnityEngine;

namespace BlindHolmes
{
    public interface IInteractable
    {
        // 視線がオブジェクトに入った瞬間に1回だけ呼ばれる
        void OnHoverEnter();

        // 視線がオブジェクトから外れた瞬間に1回だけ呼ばれる
        void OnHoverExit();

        // クリックした瞬間に呼ばれる
        void OnInteract();
    }
}
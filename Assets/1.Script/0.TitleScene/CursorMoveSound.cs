using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIのカーソル選択移動時のサウンド再生
/// </summary>
public class CursorMoveSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler, ISubmitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SoundManager.Instance.SEPlay(SE.CURSOR_MOVE);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        SoundManager.Instance.SEPlay(SE.SUBMIT);
    }
}

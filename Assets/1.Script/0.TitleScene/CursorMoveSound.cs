using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIのカーソル選択移動時のサウンド再生
/// </summary>
public class CursorMoveSound : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        SoundManager.Instance.SEPlay(SE.CURSOR_MOVE);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        SoundManager.Instance.SEPlay(SE.SUBMIT);
    }
}

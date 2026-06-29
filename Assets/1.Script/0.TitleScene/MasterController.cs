using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// タイトルシーンに設置する全デバイスの入力を受け取りまとめるコントローラー
/// </summary>
public class MasterController : MonoBehaviour
{
    [SerializeField] TitleDirector directer;

    // コントローラー ===================================================================
    // 決定
    public void OnSubmit(InputAction.CallbackContext context)
    {
        directer.EnterTitle();
    }

    // キャンセル
    public void OnCancel(InputAction.CallbackContext context)
    {
        directer.BackTitle();
    }

    // 移動
    public void OnNavigate(InputAction.CallbackContext context)
    {

    }
}

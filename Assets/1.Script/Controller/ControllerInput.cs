using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;




public class ControllerInput : MonoBehaviour
{
    [SerializeField] float deadZone;

    private PlayerInput playerInput;
    private InputActionMap playerMap;
    private InputActionMap uiMap;

    // ゲーム中の入力変数
    Vector2 moveInput;                              //移動方向
    ButtonState jump,shield;                        //ジャンプ、シールドのボタンの状態
    ButtonState atkUp, atkDown, atkLeft, atkRight;  //各攻撃のボタンの状態
    bool pauseFlag=false;                           //ポーズボタン

    // UI中の入力変数
    Vector2 navigate;                               //方向
    ButtonState submit, cancel, rButton, lButton;   //各種ボタン
    ButtonState start, menu;                        //各種ボタン


    //-----------------------------------------------------------------共通の関数

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
      playerInput = GetComponent<PlayerInput>();
        playerMap = playerInput.actions.FindActionMap("Player");
        uiMap = playerInput.actions.FindActionMap("UI");
      
    }

    /// <summary>
    /// ペアリングされているデバイスを返す
    /// </summary>
    /// <returns></returns>
    public InputDevice GetDevice()
    {
        return playerInput.devices[0];
    }

    /// <summary>
    /// 接続していたデバイスが切断されたかどうか確認する
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool CheckDeviceDisconnected(InputDevice device)
    {
        if (playerInput.devices.Contains(device)) return true;
        return false;
    }

    //Vector2->DIRECTIONDATA型
    private DIRECTIONDATA GetDirectionCommand(Vector2 input)
    {
        DIRECTIONDATA data = DIRECTIONDATA.Neutral;

        if (Mathf.Abs(input.x) > deadZone) data = input.x > 0 ? data |= DIRECTIONDATA.RIGHT : DIRECTIONDATA.LEFT;
        if (Mathf.Abs(input.y) > deadZone) data = input.y > 0 ? data |= DIRECTIONDATA.UP : DIRECTIONDATA.DOWN;

        return data;
    }


    public void ChangeControllMode(INPUT_MODE mode)
    {
        switch (mode)
        {
            case INPUT_MODE.ui:
                playerMap.Disable();
                uiMap.Enable();
            break;

            case INPUT_MODE.player:
                uiMap.Disable();
                playerMap.Enable();
            break;

            default:
                Debug.LogError("存在しないinputmodeを選択しようとしています",this);
                break;
        }

    }


    //---------------------------------------------------------------ゲーム中に呼ばれる関数

    /// <summary>
    /// ゲーム中のコントローラーの入力情報を取得する
    /// </summary>
    public InputGameData GetGameControllerData()
    {
        var dirCommand=GetDirectionCommand(moveInput);

        InputGameData data = new InputGameData()
        {
            DIRECTION_DATA = dirCommand,
            DIRECTION_VEC2 = moveInput,
            ATTACK_UP = atkUp,
            ATTACK_DOWN = atkDown,
            ATTACK_LEFT = atkLeft,
            ATTACK_RIGHT = atkRight,
            JUMP = jump,
            SHIELD=shield,
            PAUSE=pauseFlag
        };

        SetGameButtonSaveFlag();

        return data;
    }


    //現在のゲーム中の入力をセーブに入れる
    private void SetGameButtonSaveFlag()
    {
        atkUp.Save();
        atkDown.Save();
        atkLeft.Save();
        atkRight.Save();
        jump.Save();
    }

    //---------------------------------------------------------------UI操作中に呼ばれる関数

    /// <summary>
    /// UI捜査中のコントローラーの入力情報を取得する
    /// </summary>
    public InputUIData GetUIControllerData()
    {
        var dirCommand = GetDirectionCommand(navigate);

        InputUIData data = new InputUIData()
        {
            DIRECTION_DATA = dirCommand,
            DIRECTION_VEC2 = navigate,
            SUBMIT = submit,
            CANCEL = cancel,
            R_BUTTON = rButton,
            L_BUTTON = lButton,
            START = start,
            MENU = menu,
        };

        SetUIButtonSaveFlag();

        return data;
    }

    //現在のUI操作中の入力をセーブに入れる
    private void SetUIButtonSaveFlag()
    {
        submit.Save();
        cancel.Save();
        rButton.Save();
        lButton.Save();
        start.Save();
        menu.Save();
    }



    //======================================================================================================入力関係

    //------------------------------------------------------------ゲーム中の操作
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jump.Set(context.performed);
    }


    public void OnAttack_UP(InputAction.CallbackContext context)
    {
        atkUp.Set(context.performed);
    }
    public void OnAttack_DOWN(InputAction.CallbackContext context)
    {
        atkDown.Set(context.performed);
    }
    public void OnAttack_LEFT(InputAction.CallbackContext context)
    {
        atkLeft.Set(context.performed);
    }
    public void OnAttack_RIGHT(InputAction.CallbackContext context)
    {
        atkRight.Set(context.performed);
    }


    public void OnShield(InputAction.CallbackContext context)
    {
        shield.Set(context.performed);
    }


    public void OnPause(InputAction.CallbackContext context)
    {
        pauseFlag = context.performed;
    }




    //-----------------------------------------------------------UI操作中の操作

    public void OnNavigate(InputAction.CallbackContext context)
    {
       navigate = context.ReadValue<Vector2>();
    }
    public void OnSubmit(InputAction.CallbackContext context)
    {
        submit.Set(context.performed);
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
       cancel.Set(context.performed);
    }


    public void OnRButton(InputAction.CallbackContext context)
    {
        rButton.Set(context.performed);
    }
    public void OnLButton(InputAction.CallbackContext context)
    {
        lButton.Set(context.performed);
    }
    public void OnStart(InputAction.CallbackContext context)
    {
        start.Set(context.performed);
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        menu.Set(context.performed); 
    }
}

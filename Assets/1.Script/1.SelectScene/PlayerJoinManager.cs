using System;
using UnityEngine;
using UnityEngine.InputSystem;
using SelectScene;

public class PlayerJoinManager : MonoBehaviour
{
    [SerializeField] private InputAction playerJoinInputAction = default;
    [SerializeField] private PlayerInput controllerPrefab = default;
    [SerializeField] private int maxPlayerCount;

    private InputDevice[] joinedDevices = default;
    private int currentPlayerCount = 0;
    private int firstSpace = 0;


    /// <summary> プレイヤー入室時に呼ぶ(プレイヤーのIndexを渡す) </summary>
    public event Action<int,PlayerType, int> OnPlayerJoined;


    private void Awake()
    {
        joinedDevices=new InputDevice[maxPlayerCount];

        playerJoinInputAction.Enable();
        playerJoinInputAction.performed += OnJoin;
    }
    private void OnDestroy()
    {
        playerJoinInputAction.performed -= OnJoin;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        // 接続済みデバイスを全取得
        joinedDevices = ControllerInputManager.Instance.GetDevices();

        // 接続済みプレイヤーを数える
        foreach (var device in joinedDevices)
            if(device != null) currentPlayerCount++;

        TrySetFirstSpace();
    }

    // 入室
    private void OnJoin(InputAction.CallbackContext context)
    {
        // 最大人数に達していたら
        if(currentPlayerCount>= maxPlayerCount)
        {
            return;
        }

        // 同一デバイスからのリクエストなら
        foreach(var device in joinedDevices)
        {
            if (context.control.device == device) return;
        }

        // コントローラー生成
        var controller= PlayerInput.Instantiate(
            prefab: controllerPrefab.gameObject,
            playerIndex: firstSpace,
            pairWithDevice:context.control.device
            );

        GameObject obj = controller.gameObject;
        Debug.Log("join", obj);
        ControllerInputManager.Instance.AddNewController(obj);

        joinedDevices[firstSpace] = context.control.device;
        OnPlayerJoined?.Invoke(firstSpace, PlayerType.PLAYER, firstSpace);  // 登録されている処理すべてを呼び出す

        TrySetFirstSpace();

        // 接続プレイヤーを増やす
        currentPlayerCount++;
    }

    /// <summary>
    /// 退出
    /// </summary>
    /// <param name="_index">プレイヤー番号</param>
    public void OnLeft(int _index)
    {
        Debug.Log("Left" +  _index);

        // コントローラーを削除
        ControllerInputManager.Instance.RemoveController(_index);
        joinedDevices[_index] = null;

        TrySetFirstSpace();

        // 接続プレイヤーを減らす
        currentPlayerCount--;
    }

    // 次の空白を設定する
    bool TrySetFirstSpace()
    {
        bool flag = false;
        for (int i = 0; i < joinedDevices.Length; i++)
        {
            if (joinedDevices[i] != null) continue;

            firstSpace = i;
            flag = true;
            break;
        }

        return flag;
    }

    public int GetDeviceCount() { return currentPlayerCount; }
}

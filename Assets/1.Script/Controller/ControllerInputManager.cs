using UnityEngine;
using UnityEngine.InputSystem;


public enum INPUT_MODE
{
    ui=0,
    player=1
}


/// <summary>
/// すべてのコントローラーを統括するスクリプト
/// </summary>
public class ControllerInputManager : MonoBehaviour
{
    ControllerInput[] controllers;
    InputGameData?[] InputGameDatas;
    InputUIData?[] InputUIDatas;

    public static ControllerInputManager Instance{ get; private set; }

    private void Awake()
    {
        //シングルトン化
        if (Instance != null) 
        {
            //すでにほかのContollerInputManagerがある場合は削除
            Destroy(gameObject);
            return;
        }
        Instance=this;
        DontDestroyOnLoad(gameObject);

        Init();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Init()
    {
        // 最大人数分の箱を用意する
        int maxPlayer = ProjectManager.MaxPlayer;
        controllers = new ControllerInput[maxPlayer];
        InputGameDatas = new InputGameData?[maxPlayer];
        InputUIDatas = new InputUIData?[maxPlayer];


        //PlayerInputに接続しているデバイスの状態が変化した時のイベントを登録
        InputSystem.onDeviceChange += OnDeviceChange;

    }

    /// <summary>
    /// ActionMapを切り替える
    /// </summary>
    /// <param name="mode"></param>
    public void ChangeInputMode(INPUT_MODE mode)
    {
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null) continue;
            controllers[i].ChangeControllMode(mode);
        }

    }

    /// <summary>
    /// List内にあるコントローラーのゲーム中入力情報をまとめて取得する
    /// </summary>
    /// <returns></returns>
    public InputGameData?[] GetControllerDatas()
    {
        for(int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null) { InputGameDatas[i] = null; continue; }
            InputGameDatas[i] = controllers[i].GetGameControllerData();
        }

        return InputGameDatas;
    }


    /// <summary>
    /// List内にあるコントローラーのゲーム中入力情報をまとめて取得する
    /// </summary>
    /// <returns></returns>
    public InputUIData?[] GetControllerUIDatas()
    {
        for(int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null) { InputUIDatas[i] = null; continue; }
            InputUIDatas[i] = controllers[i].GetUIControllerData();
        }

        return InputUIDatas;
    }


    /// <summary>
    /// 新規コントローラーを制御下に置く
    /// </summary>
    /// <param name="obj"></param>
    public void AddNewController(GameObject obj)
    {
        if (!obj.TryGetComponent<ControllerInput>(out var cont))
        {
            Debug.LogError("加入したオブジェクトにContorllerInputが見つかりませんでした。");
            
        }
        obj.transform.parent = gameObject.transform;
    
        // 空いている箱に格納 (前から埋める)
        for(int i = 0;i < controllers.Length;i++)
        {
            if (controllers[i] != null) continue; // すでに入ってるよ

            controllers[i] = cont;
            break;
        }

        cont.Init();
    }

    /// <summary>
    /// コントローラーの削除
    /// </summary>
    /// <param name="index">プレイヤー番号</param>
    public void RemoveController(int index)
    {
        var col = controllers[index];

        if (col == null) return;

        controllers[index] = null;
        Destroy(col.gameObject);
    }

    public InputDevice[] GetDevices()
    {
        InputDevice[] devices = new InputDevice[controllers.Length];

        for(int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null) continue;

            devices[i] = controllers[i].GetDevice();
        }

        return devices;
    }

    //PlayerInputが入っているオブジェクトのデバイスが状態変化を起こしたときに呼ばれる
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected)
        {
            // すべてのコントローラーの状態を確認する
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] == null) continue; // 無効

                if (controllers[i].CheckDeviceDisconnected(device))
                {
                    Debug.Log($"プレイヤー{i}のデバイスが切断されました:{device.name}");
                    break;
                }
            }
            
        }
        else if (change == InputDeviceChange.Reconnected)
        {
            Debug.Log($"デバイスが再接続されました: {device.name}");
        }
    }


}

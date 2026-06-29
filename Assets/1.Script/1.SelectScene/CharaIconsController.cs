using SelectScene;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CharaIconsController : MonoBehaviour
{
    const int MaxPlayerCount = 4;   // どっか別の場所に書きたい

    [SerializeField] Image charaIconPrefab;     // キャラアイコンの生成用プレハブ
    [SerializeField] Transform charaIconParent; // キャラアイコンの生成先
    [SerializeField] Image[] iconFrames;        // 各プレイヤーが操作する枠（選択キャラアイコンをわかりやすくするため）
    [SerializeField] float iconDisSpace;        // キャラアイコン生成時の間の距離
    [SerializeField] CharaVisualData[] charaVisuals;

    public event Action<int, int> OnAccepted;

    Image[] charaIcons;
    Image[] frames;
    int[] framesPosNum;


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        // キャラアイコンの生成
        charaIcons = new Image[charaVisuals.Length];
        float harfWidth = (charaVisuals.Length - 1) * iconDisSpace * 0.5f;
        for (int i = 0; i < charaVisuals.Length; i++)
        {
            charaIcons[i] = Instantiate(charaIconPrefab, charaIconParent);          // 生成
            charaIcons[i].transform.Translate(i * iconDisSpace - harfWidth, 0, 0);  // ポジション設定
            charaIcons[i].sprite = charaVisuals[i].Icon;                            // sprite設定
        }
        charaIconPrefab.gameObject.SetActive(false); // プレハブは消す

        // 配列を用意
        frames = new Image[MaxPlayerCount];
        framesPosNum = new int[MaxPlayerCount];

        foreach(var frame in iconFrames) frame.gameObject.SetActive(false); // 一旦全部消す
    }

    /// <summary>
    /// キャラ選択 開始
    /// </summary>
    /// <param name="playerType">人間orCPU</param>
    /// <param name="controllerIndex">何番目のコントローラー</param>
    /// <param name="id">キャラID</param>
    public Sprite SelectCharacter(PlayerType playerType, int controllerIndex, int id = 0)
    {
        // フレームのアクティブ化　対応Indexに格納
        frames[controllerIndex] = playerType == PlayerType.PLAYER ? iconFrames[controllerIndex] : iconFrames[MaxPlayerCount]; // プレイヤー用のフレームorCPU用フレーム
        frames[controllerIndex].gameObject.SetActive(true);
        FrameMove(controllerIndex, id);

        return charaVisuals[id].BustUp;
    }

    /// <summary>
    /// 入力情報から、選択中のキャライラストを返す
    /// </summary>
    /// <returns></returns>
    public Sprite Process(int controllerIndex, SelectScene.InputData inputData)
    {
        int num = framesPosNum[controllerIndex];

        // 左入力があれば
        if (inputData.DIRECTION_DATA.HasFlag(DIRECTIONDATA.LEFT) && !inputData.DIRECTION_DATA_OLD.HasFlag(DIRECTIONDATA.LEFT))
            num = num == 0 ? charaIcons.Length - 1 : num - 1;

        // 右入力があれば
        if (inputData.DIRECTION_DATA.HasFlag(DIRECTIONDATA.RIGHT) && !inputData.DIRECTION_DATA_OLD.HasFlag(DIRECTIONDATA.RIGHT))
            num = (num + 1) % charaIcons.Length;

        // 決定なら
        if (inputData.ACCEPT) { Debug.Log("決定"); OnAccepted?.Invoke(controllerIndex, framesPosNum[controllerIndex]); }

        // ポジション移動
        FrameMove(controllerIndex, num);

        return charaVisuals[num].BustUp;
    }

    // フレームの位置を変える
    void FrameMove(int index, int num)
    {
        framesPosNum[index] = num;
        frames[index].transform.position = charaIcons[num].transform.position;
    }

    /// <summary>
    /// キャラ 決定
    /// </summary>
    /// <param name="index"></param>
    public void Accept(int index)
    {
        frames[index].gameObject.SetActive(false);
    }
}

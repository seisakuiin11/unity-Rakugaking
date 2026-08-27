using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// カスタムエディター
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    SerializedProperty bgmLoadFlag;
    SerializedProperty seLoadFlag;
    SerializedProperty charaseLoadFlag;
    SerializedProperty narboLoadFlag;
    SerializedProperty slasherLoadFlag;
    SerializedProperty boyLoadFlag;
    SerializedProperty zeraLoadFlag;

    private void OnEnable()
    {
        // 紐づけ
        bgmLoadFlag = serializedObject.FindProperty("bgmLoadFlag");
        seLoadFlag = serializedObject.FindProperty("seLoadFlag");
        charaseLoadFlag = serializedObject.FindProperty("charaseLoadFlag");
        narboLoadFlag = serializedObject.FindProperty("narboLoadFlag");
        slasherLoadFlag = serializedObject.FindProperty("slasherLoadFlag");
        boyLoadFlag = serializedObject.FindProperty("boyLoadFlag");
        zeraLoadFlag = serializedObject.FindProperty("zeraLoadFlag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 指定項目以外を表示
        DrawPropertiesExcluding(serializedObject,
            "bgmLoadFlag",
            "seLoadFlag",
            "charaseLoadFlag",
            "narboLoadFlag",
            "slasherLoadFlag",
            "boyLoadFlag",
            "zeraLoadFlag"
        );

        // 描画
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ロードする音素材", EditorStyles.boldLabel);
        bgmLoadFlag.boolValue = EditorGUILayout.ToggleLeft("BGM", bgmLoadFlag.boolValue);
        seLoadFlag.boolValue = EditorGUILayout.ToggleLeft("SE", seLoadFlag.boolValue);
        charaseLoadFlag.boolValue = EditorGUILayout.ToggleLeft("キャラSE", charaseLoadFlag.boolValue);

        // falseなら編集不可
        EditorGUI.BeginDisabledGroup(!charaseLoadFlag.boolValue);

        // 対象となる変数 描画
        narboLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ナルボ", narboLoadFlag.boolValue);
        slasherLoadFlag.boolValue = EditorGUILayout.ToggleLeft("スラッシャー", slasherLoadFlag.boolValue);
        boyLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ボーイ", boyLoadFlag.boolValue);
        zeraLoadFlag.boolValue = EditorGUILayout.ToggleLeft("ゼーラ", zeraLoadFlag.boolValue);

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}

#endif

/// <summary>
/// BGM
/// </summary>
public enum BGM : byte
{
    TITLE,
    SELECT,
    RESULT,
    NARBO,
    ZERA,
    BOY,
    SLASHER,
    MAX
}

/// <summary>
/// SE
/// </summary>
public enum SE
{
    TITLE_ENTER,
    CURSOR_MOVE,
    MEKURU,
    SUBMIT,
    BACK,
    SHOW_WINDOW,
    PLAYER_ENTRY,
    CHARA_ACCEPT,
    BATTLE_START,
    BATTLE_END,
    RESULT_JINGLE,
    MAX
}

/// <summary>
/// キャラクターSE
/// </summary>
public enum CHARASE
{   // 共通SE ---------------------------
    JUMP,
    HIT_1, HIT_2, HIT_3,
    GUARD_1, GUARD_2, GUARD_CRASH,
    PIYOPIYO,
    NARBO_ZINARASI, // ナルボ -----------
    NARBO_STRIKE,
    NARBO_WASHOI,
    NARBO_DAIBAKUHATU,
    SLASHER_KERIAGE, // スラッシャー ----
    SLASHER_DIVESOBAT,
    SLASHER_SOMERSAULT,
    SLASHER_BREAKTHROUGH,
    SLASHER_ACCEL,
    MAX
}

public class SoundManager : MonoBehaviour
{
    const string NARBO = "NARBO", SLASHER = "SLASHER", BOY = "BOY", ZERA = "ZERA";

    public static SoundManager Instance { get; private set; }

    [SerializeField] AudioSource BGMPlayer;
    [SerializeField] AudioSource SEPlayer;

    [SerializeField] bool bgmLoadFlag;
    [SerializeField] bool seLoadFlag;
    [SerializeField] bool charaseLoadFlag;
    [SerializeField] bool narboLoadFlag;
    [SerializeField] bool slasherLoadFlag;
    [SerializeField] bool boyLoadFlag;
    [SerializeField] bool zeraLoadFlag;

    Dictionary<BGM, AudioClip> bgm;
    Dictionary<BGM, AsyncOperationHandle<AudioClip>> bgmHandles;
    Dictionary<SE, AudioClip> se;
    Dictionary<SE, AsyncOperationHandle<AudioClip>> seHandles;
    Dictionary<CHARASE, AudioClip> charaSE;
    Dictionary<CHARASE, AsyncOperationHandle<AudioClip>> charaSEHandles;

    int[] sePlayFlag;
    int[] charaSEPlayFlag;


    private void Awake()
    {
        //すでにほかのSoundManagerがある場合は削除
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //シングルトン化
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task Init(SceneName scene)
    {
        Debug.Log("Scene : " + scene.ToString());

        await LoadBGM();
        await LoadSE();
        await LoadCharaSE();

        Debug.Log("サウンド素材 ロード完了");

        // OneShot管理用
        sePlayFlag = new int[(int)SE.MAX];
        charaSEPlayFlag = new int[(int)CHARASE.MAX];
    }

    // BGM素材を取得
    async Task LoadBGM()
    {
        bgmHandles = new();
        bgm = new();

        if (!bgmLoadFlag){ Debug.LogWarning("BGMのロードをスキップ"); return; }

        // 素材を取得する存在を用意
        var handles = new List<AsyncOperationHandle<AudioClip>>();
        for (int i = 0; i < (int)BGM.MAX; i++)
            handles.Add(Addressables.LoadAssetAsync<AudioClip>(((BGM)i).ToString()));

        // 素材の取得
        try
        {
            await Task.WhenAll(handles.Select(h => h.Task));
        }
        catch { } // 失敗した場合でも続行

        // 取得した素材を保存
        for(int i = 0; i < handles.Count; i++)
        {
            // ロードに失敗したなら
            if (handles[i].Status != AsyncOperationStatus.Succeeded) { Debug.LogWarning($"BGM {((BGM)i)} のロードに失敗"); continue; }

            bgmHandles.Add((BGM)i,handles[i]);
            bgm.Add((BGM)i, handles[i].Result);
        }
    }

    // SE素材を取得
    async Task LoadSE()
    {
        seHandles = new();
        se = new();

        if (!seLoadFlag) { Debug.LogWarning("SEのロードをスキップ"); return; }

        // 素材を取得する存在を用意
        var handles = new List<AsyncOperationHandle<AudioClip>>();
        for (int i = 0; i < (int)SE.MAX; i++)
            handles.Add(Addressables.LoadAssetAsync<AudioClip>(((SE)i).ToString()));

        // 素材の取得
        try
        {
            await Task.WhenAll(handles.Select(h => h.Task));
        }
        catch { } // 失敗した場合でも続行

        // 取得した素材を保存
        for (int i = 0; i < handles.Count; i++)
        {
            if (handles[i].Status != AsyncOperationStatus.Succeeded) { Debug.LogWarning($"SE {((SE)i)} のロードに失敗"); continue; }

            seHandles.Add((SE)i, handles[i]);
            se.Add((SE)i, handles[i].Result);
        }
    }

    // キャラSE素材を取得
    async Task LoadCharaSE()
    {
        charaSEHandles = new();
        charaSE = new();

        if (!charaseLoadFlag) { Debug.LogWarning("キャラSEのロードをスキップ"); return; }

        // 素材を取得する存在を用意
        var handles = new List<AsyncOperationHandle<AudioClip>>();
        for (int i = 0; i < (int)CHARASE.MAX; i++)
        {
            // ナルボに関する音素材を取得しない
            if (!narboLoadFlag && ((CHARASE)i).ToString().Contains(NARBO)) continue;
            // スラッシャーに関する音素材を取得しない
            if (!slasherLoadFlag && ((CHARASE)i).ToString().Contains(SLASHER)) continue;
            // ボーイに関する音素材を取得しない
            if (!boyLoadFlag && ((CHARASE)i).ToString().Contains(BOY)) continue;
            // ゼーラに関する音素材を取得しない
            if (!zeraLoadFlag && ((CHARASE)i).ToString().Contains(ZERA)) continue;


            handles.Add(Addressables.LoadAssetAsync<AudioClip>(((CHARASE)i).ToString()));
        }

        // 素材の取得
        try
        {
            await Task.WhenAll(handles.Select(h => h.Task));
        }
        catch { } // 失敗した場合でも続行

        // 取得した素材を保存
        for (int i = 0; i < handles.Count; i++)
        {
            if (handles[i].Status != AsyncOperationStatus.Succeeded) { Debug.LogWarning($"キャラSE {((CHARASE)i)} のロードに失敗"); continue; }

            charaSEHandles.Add((CHARASE)i, handles[i]);
            charaSE.Add((CHARASE)i, handles[i].Result);
        }
    }

    /// <summary>
    /// BGMを再生
    /// </summary>
    /// <param name="bgmName"></param>
    public void BGMPlay(BGM bgmName)
    {
        // 素材があるか確認
        if (!bgm.TryGetValue(bgmName, out var source)) { Debug.LogWarning($"BGM {bgmName} がありません。"); return; }

        BGMPlayer.clip = source;
        BGMPlayer.Play();
    }

    /// <summary>
    /// BGMのランダム再生
    /// </summary>
    /// <param name="bgmNames"></param>
    public void RandomBGMPlay(BGM[] bgmNames)
    {
        int i = Random.Range(0, bgmNames.Length);
        BGMPlay(bgmNames[i]);
    }

    /// <summary>
    /// SEを再生(重複再生可能)
    /// </summary>
    /// <param name="seName"></param>
    public void SEPlay(SE seName)
    {
        // 素材があるか確認
        if(!se.TryGetValue(seName, out var source)) { Debug.LogWarning($"SE {seName} がありません。"); return; }

        // 同一フレーム内での再生を防ぐ
        if (sePlayFlag[(int)seName] == Time.frameCount) return;

        sePlayFlag[(int)seName] = Time.frameCount;

        SEPlayer.PlayOneShot(source);
    }

    /// <summary>
    /// キャラSEを再生(重複再生可能)
    /// </summary>
    /// <param name="seName"></param>
    public void CharaSEPlay(CHARASE seName)
    {
        // 素材があるか確認
        if (!charaSE.TryGetValue(seName, out var source)) { Debug.LogWarning($"キャラSE {seName} がありません。"); return; }

        // 同一フレーム内での再生を防ぐ
        if (charaSEPlayFlag[(int)seName] == Time.frameCount) return;

        charaSEPlayFlag[(int)seName] = Time.frameCount;

        SEPlayer.PlayOneShot(source);
    }

    // アセットの開放
    private void OnDestroy()
    {
        // BGM素材
        foreach (var handle in bgmHandles.Values)
            Addressables.Release(handle);

        bgmHandles.Clear();
        bgm.Clear();

        // SE素材
        foreach (var handle in seHandles.Values)
            Addressables.Release(handle);

        seHandles.Clear();
        se.Clear();

        // キャラSE素材
        foreach (var handle in charaSEHandles.Values)
            Addressables.Release(handle);
        charaSEHandles.Clear();
        charaSE.Clear();
    }
}

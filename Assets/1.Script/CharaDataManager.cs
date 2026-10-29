using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

/// <summary>
/// キャラクターに関連するデータを管理するクラス
/// </summary>
public class CharaDataManager : MonoBehaviour
{
    /// <summary>
    /// キャラID
    /// </summary>
    public enum CharaID : byte
    {
        NARBO,
        SLASHER,
        BOY,
        ZERA,
        MODEL_1,
        MAX
    }

    private class SkinPngData
    {
        public string DirectoryPath;
        public byte[] All;
        public byte[] BustUp;
        public byte[][] Layers;
    }

    public static CharaDataManager Instance { get; private set; }

    [SerializeField] CharacterProfile[] characterProfiles;
    [SerializeField] CharaVisualData[] charaVisualDatas;
    [SerializeField] CharacterController[] characterPrefabs;

    [SerializeField] CharacterProfile errorProfile;
    [SerializeField] CharaVisualData errorVisualData;
    [SerializeField] CharacterController errorPrefab;

    [SerializeField] List<ModelVisualData> modelVisualDatas = new();

    // 保存先のパス
#if UNITY_EDITOR
    string rootDirectory = Path.Combine( Application.dataPath, "SaveData", "Skins" );
#else
    string rootDirectory = Path.Combine( Application.persistentDataPath, "Skins" );
#endif


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //すでにほかのContollerInputManagerがある場合は削除
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //シングルトン化
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 初期化処理 データのロード
    /// </summary>
    /// <returns></returns>
    public async Task Init()
    {
        await LoadSkins();
    }

    // スキンデータをロードする
    async Task LoadSkins()
    {
        // 保存先が存在しなければ終了
        if (!Directory.Exists(rootDirectory))
        {
            Debug.Log("保存済みスキンはありません。");
            return;
        }

        // 既存のリストをクリアして重複を防ぐ
        //modelVisualDatas.Clear();

        // CustomSkin1、CustomSkin2...を取得して番号順に並べる
        var directories = Directory.GetDirectories( rootDirectory, "CustomSkin*" )
        .Select(path => new // 各フォルダのパスと番号を組み合わせる
        {
            Path = path,
            Number = GetSkinNumber(path) // フォルダ名から番号を取り出す
        })
        .Where(x => x.Number > 0)       // 正しい番号を持つフォルダだけを残す
        .OrderBy(x => x.Number)         // 番号の小さい順に並べる
        .ToArray();                     // 結果を配列として確定する

        SkinPngData[] loadedData = await Task.WhenAll(
            directories.Select(x => ReadSkinBytesAsync(x.Path))
        );

        // セーブデータをすべて格納していく
        foreach (var pngData in loadedData)
        {
            ModelVisualData skin = ScriptableObject.CreateInstance<ModelVisualData>();

            // 全身画像
            skin.KeyVisual = LoadSpriteFromBytes( pngData.All, "All.png" );

            // バストアップ画像
            skin.BustUp = LoadSpriteFromBytes( pngData.BustUp, "BustUp.png" );

            // 各部位の画像
            for (int i = 0; i < (int)ModelVisualData.LayerType.MAX; i++)
            {
                var layerType = (ModelVisualData.LayerType)i;
                skin.partSprite[i] = LoadSpriteFromBytes(pngData.Layers[i], $"Layer_{layerType}.png");
            }

            // 読み込んだスキンを格納
            modelVisualDatas.Add(skin);

            Debug.Log($"スキンをロードしました: {pngData.DirectoryPath}");
        }

        Debug.Log($"ロード完了：{modelVisualDatas.Count} 件");
    }

    private async Task<SkinPngData> ReadSkinBytesAsync(string directory)
    {
        // 全身,バストアップ,各部位
        var tasks = new Task<byte[]>[(int)ModelVisualData.LayerType.MAX + 2];

        // 全身とバストアップ
        tasks[0] = ReadPngAsync( Path.Combine(directory, "All.png") );

        tasks[1] = ReadPngAsync( Path.Combine(directory, "BustUp.png") );

        // 各部位
        for (int i = 0; i < (int)ModelVisualData.LayerType.MAX; i++)
        {
            var type = (ModelVisualData.LayerType)i;
            tasks[i + 2] = ReadPngAsync( Path.Combine(directory, $"Layer_{type}.png"));
        }

        // すべてのファイル読み込みを待つ
        byte[][] bytes = await Task.WhenAll(tasks);

        // 読み込み結果を格納
        var result = new SkinPngData
        {
            DirectoryPath = directory,
            All = bytes[0],
            BustUp = bytes[1],
            Layers = new byte[(int)ModelVisualData.LayerType.MAX][]
        };

        // 各部位のデータを格納
        for (int i = 0; i < result.Layers.Length; i++)
        {
            result.Layers[i] = bytes[i + 2];
        }

        return result;
    }

    // 画像データを取得する
    private async Task<byte[]> ReadPngAsync(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"画像ファイルがありません: {path}");
            return null;
        }

        try
        {
            // ファイルの読み込みをバックグラウンドで実行
            return await Task.Run(() => File.ReadAllBytes(path));
        }
        catch (Exception e)
        {
            Debug.LogError($"画像ファイルの読み込みに失敗: {path}\n{e}");
            return null;
        }
    }

    // ファイルのパスから数字だけを取り出す
    private int GetSkinNumber(string directoryPath)
    {
        // ファイル名を取得
        string directoryName = Path.GetFileName(directoryPath);

        const string prefix = "CustomSkin";

        if (!directoryName.StartsWith(prefix)) return -1;

        // 終端文字 数字を取得する
        string numberText = directoryName.Substring(prefix.Length);

        // 数字に変換
        return int.TryParse(numberText, out int number) ? number : -1;
    }

    // 画像データからSpriteを取得する
    private Sprite LoadSpriteFromBytes(byte[] pngData, string spriteName)
    {
        if (pngData == null || pngData.Length == 0)
            return null;

        Texture2D texture = new Texture2D(
            2, 2,
            TextureFormat.RGBA32,
            false
        );

        if (!texture.LoadImage(pngData))
        {
            Debug.LogError($"PNGの展開に失敗: {spriteName}");
            Destroy(texture);
            return null;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        sprite.name = spriteName;

        return sprite;
    }

    /// <summary>
    /// キャラのテキストデータを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterProfile GetCharacterProfile(int id)
    {
        if (id < 0 || id >= characterProfiles.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorProfile; }

        if (characterProfiles[id] == null) { Debug.LogError("Profileがありません。"); return errorProfile; }

        return characterProfiles[id];
    }
    public CharacterProfile GetCharacterProfile(CharaID id)
    {
        int _id = (int)id;
        return GetCharacterProfile(_id);
    }
    
    /// <summary>
    /// キャラのビジュアル系データを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharaVisualData GetVisualData(int id, int skinNum)
    {
        if (id < 0 || id >= charaVisualDatas.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorVisualData; }

        if (charaVisualDatas[id] == null) { Debug.LogError("ビジュアルデータがありません。"); return errorVisualData; }

        // モデル以外のキャラなら
        if(id < (int)CharaID.MODEL_1) return charaVisualDatas[id];

        // 仮の処理
        charaVisualDatas[id].KeyVisual = modelVisualDatas[skinNum].KeyVisual;
        charaVisualDatas[id].BustUp = modelVisualDatas[skinNum].BustUp;

        return charaVisualDatas[id];
    }
    public CharaVisualData GetVisualData(CharaID id, int skinNum)
    {
        int _id = (int)id;
        return GetVisualData(_id, skinNum);
    }

    /// <summary>
    /// キャラのビジュアル系データをすべて返す
    /// </summary>
    /// <returns></returns>
    public CharaVisualData[] GetAllVisualDatas()
    {
        return charaVisualDatas;
    }

    /// <summary>
    /// キャラのPrefabを返す
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterController GetCharaPrefab(int id)
    {
        if(id < 0 || id >= characterPrefabs.Length) { Debug.LogError($"ID{id}は存在しません。"); return errorPrefab; }

        if (characterPrefabs[id] == null) { Debug.LogError("Prefabがありません。"); return errorPrefab; }

        return characterPrefabs[id];
    }
    public CharacterController GetCharaPrefab(CharaID id)
    {
        int _id = (int)id;
        return GetCharaPrefab(_id);
    }

    /// <summary>
    /// スキンを追加する 保存する
    /// </summary>
    /// <param name="skin"></param>
    public void SaveSkin(ModelVisualData skin)
    {
        modelVisualDatas.Add(skin);

        // 保存先フォルダ
        string saveDirectory = Path.Combine( rootDirectory, $"CustomSkin{modelVisualDatas.Count}" );

        Directory.CreateDirectory(saveDirectory);

        // 全身画像を保存
        SaveSpriteAsPng( skin.KeyVisual, Path.Combine(saveDirectory, "All.png") );

        // バストアップ画像を保存
        SaveSpriteAsPng( skin.BustUp, Path.Combine(saveDirectory, "BustUp.png") );

        // 各部位の画像も保存
        for (int i = 0; i < skin.partSprite.Length; i++)
        {
            SaveSpriteAsPng( skin.partSprite[i], Path.Combine(saveDirectory, $"Layer_{(ModelVisualData.LayerType)i}.png") );
        }

        Debug.Log($"スキンを保存しました: {saveDirectory}");
    }

    // 指定パスにpng画像を保存する
    private void SaveSpriteAsPng(Sprite sprite, string path)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Spriteがnullのため保存できません: {path}");
            return;
        }

        Texture2D texture = sprite.texture;
        byte[] pngData = texture.EncodeToPNG();

        File.WriteAllBytes(path, pngData);
    }

    /// <summary>
    /// skinNumにvaluel追加して、スキン個数を越えないようにする
    /// </summary>
    /// <param name="charaID">キャラID</param>
    /// <param name="skinNum">現在のスキンID</param>
    /// <param name="value">追加する値</param>
    /// <returns></returns>
    public int SkinNumShift(int charaID, int skinNum, int value)
    {
        skinNum += value;

        if(skinNum >= modelVisualDatas.Count)
            return skinNum - modelVisualDatas.Count;

        if (skinNum < 0)
            return modelVisualDatas.Count + skinNum;

        return skinNum;
    }

    /// <summary>
    /// モデルのKVや各部位のデータ箱を返す
    /// </summary>
    /// <param name="skinNum"></param>
    /// <returns></returns>
    public ModelVisualData GetModelVisualData(int skinNum)
    {
        return modelVisualDatas[skinNum];
    }

    /// <summary>
    /// モデルの全KVや各部位のデータ箱を返す
    /// </summary>
    /// <returns></returns>
    public ModelVisualData[] GetAllModelVisualData()
    {
        return modelVisualDatas.ToArray();
    }
}

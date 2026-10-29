using System.IO;
using UnityEngine;
using System.Collections.Generic;




/// <summary>
/// キャラクターのセーブデータをまとめたもの
/// </summary>
public struct SaveCharaDatas
{
    public List<SaveCharaData> charaData;
}


/// <summary>
/// キャラクター情報を読み込むもの
/// </summary>
public class CharaSaveDirector : MonoBehaviour
{
    private string savePath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //ファイルパスの作成
        savePath = Path.Combine(Application.persistentDataPath, "characters.json");
    }

    // Update is called once per frame
    void Update()
    {
        
    }





    /// <summary>
    /// キャラクターのデータを保存する
    /// </summary>
    /// <param name="_data"></param>
    public void SaveChara(List<SaveCharaData> _data)
    {
        

        SaveCharaDatas _savedata = new SaveCharaDatas { charaData = _data };
        string _json = JsonUtility.ToJson(_data);
        File.WriteAllText("chara",_json);
    }

    /// <summary>
    /// キャラクターのデータを取得する
    /// </summary>
    /// <returns></returns>
    public List<SaveCharaData> LoadChara()
    {
        //セーブデータがない場合は除外
        if (!File.Exists(savePath)) return null;

        string json = File.ReadAllText(savePath);

        SaveCharaDatas _data= JsonUtility.FromJson<SaveCharaDatas>(json);

        return _data.charaData;
    }
}

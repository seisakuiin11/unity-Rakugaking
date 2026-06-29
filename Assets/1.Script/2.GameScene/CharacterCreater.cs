using UnityEngine;

public class CharacterCreater : MonoBehaviour
{
    [Header("キャラクター プレハブ")]
    [SerializeField] CharacterController charPrefab;
    [Header("生成の基準点")]
    [SerializeField] Vector3 createCenterPos;
    [Header("生成した時のキャラクター同士の距離")]
    [SerializeField] float createDistance;


    /// <summary>
    /// 指定人数分のキャラクターを生成
    /// </summary>
    /// <param name="num">人数</param>
    /// <returns>生成したキャラクター達を配列で返す</returns>
    public CharacterController[] CreateCharacters(int num)
    {
        // 作り必要がない
        if(num  == 0) return null;

        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        var characters = new CharacterController[num];
        float harfLength = createDistance * (num - 1) * 0.5f;

        // 指定人数分生成する
        for(int i  = 0; i < num; i++)
        {
            var obj = Instantiate(charPrefab);  // 生成
            obj.transform.position = createCenterPos;   // 原点
            obj.transform.Translate((createDistance * i) - harfLength, 0, 0); // ずらす
            characters[i] = obj;
        }

        return characters;
    }
}

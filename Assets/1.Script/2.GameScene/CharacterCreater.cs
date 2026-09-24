using UnityEngine;

public class CharacterCreater : MonoBehaviour
{
    [Header("生成の基準点")]
    [SerializeField] Vector3 createCenterPos_2P;
    [SerializeField] Vector3 createCenterPos_4P;
    [Header("生成した時のキャラクター同士の距離")]
    [SerializeField] float createDistance;


    /// <summary>
    /// 指定人数分のキャラクターを生成
    /// </summary>
    /// <param name="maxPlayer">参加人数</param>
    /// <returns>生成したキャラクター達を配列で返す</returns>
    public CharacterController[] CreateCharacters(PlayerType[] types, int[] charaIDs, int maxPlayer)
    {
        // 参加人数が2人以下かそうでないか
        Vector3 createCenterPos = maxPlayer <= 2 ? createCenterPos_2P : createCenterPos_4P;
        var characters = new CharacterController[types.Length];
        float harfLength = createDistance * (maxPlayer - 1) * 0.5f;

        int num = 0;
        for(int i  = 0; i < types.Length; i++)
        {
            // 無人なら次へ
            if (types[i] == PlayerType.NONE) continue;

            var prefab = CharaDataManager.Instance.GetCharaPrefab(charaIDs[i]); // Prefabを取得

            if(prefab == null) continue;

            var obj = Instantiate(prefab);  // 生成
            obj.transform.position = createCenterPos;   // 原点
            obj.transform.Translate((createDistance * num) - harfLength, 0, 0); // ずらす
            characters[i] = obj;
            num++;
        }

        return characters;
    }
}

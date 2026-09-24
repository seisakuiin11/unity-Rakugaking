using UnityEngine;

/// <summary>
/// プレイヤー人数に合わせて、フィールドを管理する
/// </summary>
public class FieldController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] float cameraScale_2P;
    [SerializeField] float cameraScale_4P;
    [SerializeField] GameObject field_2P;
    [SerializeField] GameObject field_4P;


    /// <summary>
    /// 参加人数に合わせて、ステージを準備する
    /// </summary>
    /// <param name="maxPlayer">参加人数</param>
    public void SetField(int maxPlayer)
    {
        // 参加人数が2人以下なら
        if (maxPlayer <= 2) SetField2P();
        // 3人以上なら
        else SetField4P();
    }

    void SetField2P()
    {
        _camera.orthographicSize = cameraScale_2P;
        field_2P?.SetActive(true);
        field_4P?.SetActive(false);
    }

    void SetField4P()
    {
        _camera.orthographicSize = cameraScale_4P;
        field_4P?.SetActive(true);
        field_2P?.SetActive(false);
    }
}

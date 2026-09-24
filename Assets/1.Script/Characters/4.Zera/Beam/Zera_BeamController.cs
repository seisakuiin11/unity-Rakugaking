
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Zera_BeamController : MonoBehaviour
{

    [SerializeField] float beamMaxSize=5f;
    [SerializeField] float beamMinSize = 1f;
    [SerializeField] float beamSizeUpValue = 0.5f;

    [SerializeField] GameObject beam;
    [SerializeField] Transform energyImg;
    [SerializeField] List<CircleColData> beamColData;
    [SerializeField] AttackData beamAtkData;
    AttackColliderList beamColList;

    ColliderManager colManager;

    private bool chargeFlag;
    private bool endFlag;
    private int stack;

    public void BeamInit(ColliderManager _colManager)
    {
        colManager = _colManager;
        BeamDataReset();
    }

    /// <summary>
    /// チャージ開始
    /// </summary>
    public void ChargeStart()
    {
        chargeFlag = true;
        energyImg.gameObject.SetActive(true);
        BeamStackUp();
    }

    /// <summary>
    /// ビームを発射する
    /// </summary>
    public void BeamShot()
    {
        chargeFlag = false;
        beam.SetActive(true);
        energyImg.gameObject.SetActive(false);

        float value = stack * beamSizeUpValue + beamMinSize;
        if(value > beamMaxSize) value = beamMaxSize;

        //サイズを拡大
        transform.localScale = new Vector3(value, value, transform.localScale.z);

        //コライダーを生成
        ColliderSet();
    }

    /// <summary>
    /// 当たり判定を無くす　非アクティブ化
    /// </summary>
    public void BeamInactive()
    {
        endFlag = true;
        colManager.DestroyCircleCol(beamColList);
    }

    /// <summary>
    /// ビーム 終了
    /// </summary>
    public void BeamEnd()
    {
        // 非アクティブ状態でなければ
        if(!endFlag) colManager.DestroyCircleCol(beamColList);
        stack = 0;
        BeamDataReset();
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="chara"></param>
    /// <param name="col"></param>
    public void BeamHitAction(CharacterController chara, Collider col)
    {

        chara.Damage(beamAtkData.AtkDamage, beamAtkData.AtkHitStanFrame, beamAtkData.KnockBackData[0], col);
    }

    /// <summary>
    /// スケールのリセット
    /// </summary>
    private void BeamDataReset()
    {
        endFlag = false;
        beam.SetActive(false);
        energyImg.gameObject.SetActive(false);
        transform.localScale = new Vector3(beamMinSize, beamMinSize, transform.localScale.z);
    }

    /// <summary>
    /// スタックを貯める
    /// </summary>
    public void BeamStackUp()
    {
        if (!chargeFlag) return;

        stack++;
        float value = stack * beamSizeUpValue;
        if (value > beamMaxSize - beamMinSize) value = beamMaxSize - beamMinSize;
        value *= 0.8f;
        energyImg.localScale = new Vector3(value, value, energyImg.localScale.z);
    }

    /// <summary>
    /// コライダーの生成
    /// </summary>
    private void ColliderSet()
    {
        float scaleY = gameObject.transform.lossyScale.x < 0 ? -1 : 1;
 
        List<CircleColData> _beamColData = new();
        //データに格納されているコライダーの数分行われる
        for (int i = 0; i < beamColData.Count; i++)
        {
            var colData = beamColData[i];
            //Transformを格納
            colData.trans = gameObject.transform;


            //サイズを対象のTransform.LocalScaleのY軸に合わせる
            colData.radius *= gameObject.transform.localScale.y;

           
            //位置関係を対象のTransform.LocalScaleのY軸に合わせる
            colData.localPos = new Vector3(
                colData.localPos.x * scaleY,
                colData.localPos.y * scaleY,
                colData.localPos.z * scaleY
                );
            _beamColData.Add(colData);
        }
        beamColList = colManager.UpdateAttackColliderData(_beamColData, BeamHitAction);
    }
}


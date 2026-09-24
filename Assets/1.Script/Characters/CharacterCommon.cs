using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// キャラクターを実際に動かすスクリプト
/// </summary>
public class CharacterCommon : MonoBehaviour
{

    CharacterController charaCon;
    Rigidbody2D rb;
    [SerializeField] CapsuleCollider2D capsuleCol;
    [SerializeField] GameObject shieldObject;

    private float moveSpeed;                     //速度
    private float jumpForce;                     //ジャンプの高さ
    private float sideMoveDeadZone = 0.25f;      //横移動のデッドゾーン
    private int hp;                              //キャラクターの体力
    private int maxHp;                           //キャラクターの最大体力


    public int stanFrame;    //スタンする時間
    private int maxShield;   //シールドの最大値
    public int currentShield { get; private set; }                            //シールドの現在値
    public int shieldBreakStanFlame { get; private set; } = 300;              //シールドブレイクによるスタンフレーム数
    public float shieldDamageMultiply { get; private set; } = 0.1f;           //hpダメージ → シールドダメージへの変換倍率
    public Vector2 shieldBreakKnockBack = new(0, 30);                         //シールドブレイクによるノックバックベクトル:



  

    //当たり判定関係
    private HitBoxColliderList hitboxColList;     //現在このプレイヤーについている当たり判定
    private PushBox_Circle pushBox;               //現在このプレイヤーについている押し出しコライダー
    [NonSerialized] public ColliderManager colManager;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController _charaCon, Rigidbody2D _rb,CharacterData _charaData,ColliderManager _colManager)
    {
        //キャラクターの値挿入
        charaCon = _charaCon;
        rb= _rb;
        moveSpeed = _charaData.moveSpeed;
        maxShield=_charaData.maxShield;
        maxHp = _charaData.maxHp;
        hp = maxHp;
        jumpForce = _charaData.jumpForce;

        colManager= _colManager;

        currentShield = maxShield;

        //当たり判定をデフォルトに設定
        HitBoxDefaultSet();

        //押し出しコライダーの初期設定
        CircleColData _pushBox = new();
        _pushBox.localPos = Vector3.zero;
        _pushBox.radius = 0.5f;
        _pushBox.trans = gameObject.transform;

        //押し出しコライダーを生成
        pushBox = colManager.UpdatePushBoxColliderData(_pushBox, this);
    }



    /// <summary>
    /// 移動速度が固定化された移動処理（Animation関係なし、動くだけ）
    /// </summary>
    /// <param name="value"></param>
    public void FixedMoveAction(Vector2 value)
    {

        var v = rb.linearVelocity;

        //入力がデッドゾーン以下なら移動しない
        if (Mathf.Abs(value.x) <= sideMoveDeadZone) return;


        // 入力から方向を調べ、進行方向にする
        var direction = value.x >= 0 ? 1 : -1;

        v.x = direction * moveSpeed * Time.deltaTime;

        rb.linearVelocity = v;


        if (value.x != 0) DirectionChange(value.x);
    }

    /// <summary>
    /// 移動速度が固定化されていない移動処理（Animation関係なし、動くだけ）
    /// </summary>
    /// <param name="value"></param>
    public void MoveAction(Vector2 value)
    {

        var v = rb.linearVelocity;


        //入力がデッドゾーン以下なら移動しない
        if (Mathf.Abs(value.x) <= sideMoveDeadZone) return;

        v.x = value.x * moveSpeed * Time.deltaTime;

        rb.linearVelocity = v;

        if (value.x != 0) DirectionChange(value.x);

    }

    /// <summary>
    /// プレイヤーの向きを変更する
    /// </summary>
    /// <param name="xValue">x方向の値</param>
    public void DirectionChange(float xValue)
    {


        // 入力から方向を調べ、進行方向にする
        var direction = xValue >= 0 ? 1 : -1;

        //進行方向からプレイヤーの向きを変更
        transform.localScale = new Vector3(direction, 1, 1);
    }

    public void Move(Vector2 value)
    {
        transform.position = new Vector3(transform.position.x + value.x, transform.position.y + value.y);
    }


    /// <summary>
    /// 足場に足がついているか確認する
    /// </summary>
    /// <returns></returns>
    public bool FootingGroundCheck(out Collider2D col)
    {

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();


        var pColHeightHalf = pCol.size.y / 2;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

        col = null;

        //rayを飛ばし何もなかったら抜ける
        if (hit.collider == null) return false;

        if (hit.collider.tag != "Footing") return false;


        col = hit.collider;

        return true;




    }

    /// <summary>
    /// 足場を降りる
    /// </summary>
    /// <param name="footingCol"></param>
    public void FootingGroundOff(Collider2D footingCol)
    {

        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();

        StartCoroutine(FootingGroundOFF(pCol, footingCol));
    }

    private IEnumerator FootingGroundOFF(Collider2D iCol, Collider2D footingCol)
    {
        Physics2D.IgnoreCollision(iCol, footingCol, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(iCol, footingCol, false);
    }


    /// <summary>
    /// プレイヤーのサイズ変更
    /// </summary>
    /// <param name="scale"></param>
    public void ScaleChange(Vector3 scale)
    {
        //現在の向いている方向にx軸を合わせる
        if (transform.localScale.x < 0) scale.x *= -1f;
        transform.localScale = scale;
    }






    /// <summary>
    /// その場で止まる
    /// </summary>
    public void StopXMove()
    {
        var v = rb.linearVelocity;
        v.x = 0;
        rb.linearVelocity = v;
    }





    /// <summary>
    /// HPの値をvalue分増やし反映させる
    /// </summary>
    /// <param name="value"></param>
    public void HPChange(int value)
    {
        hp += value;

        //最大以上にはならないように
        if (hp >= maxHp) hp = maxHp;

        //UIを反映
        charaCon.HPUIChange(hp);

        //０以下なら死亡
        if (hp <= 0) Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        HitBoxReset();
        PushBoxReset();
        charaCon.Dead();
    }

    /// <summary>
    /// 復活する
    /// </summary>
    public void Revive()
    {

        hp = maxHp;
        HitBoxDefaultSet();

        CircleColData _pushBox = new();
        _pushBox.localPos = Vector3.zero;
        _pushBox.radius = 0.5f;
        _pushBox.trans = gameObject.transform;

        List<CircleColData> hitboxList = new();
        pushBox = colManager.UpdatePushBoxColliderData(_pushBox, this);
    }


    /// <summary>
    /// 吹っ飛ぶ
    /// </summary>
    /// <param name="_data">ノックバックの情報</param>
    /// <param name="pos"></param>
    public void KnockBack(KnockBackData _data, Collider _col)
    {



        //---------吹っ飛び方が指定ベクトルの場合

        if (_data.Vector == VectorType.Const)
        {
            ConstKnockBack(_data.KnockBackFlip, _data.KnockBackVec, _data.KnockBackPower, _col);
            return;
        }



        //----------------------放射状に飛ぶ場合
        if (_data.Vector == VectorType.Radial)
        {
            RadialKnockBack(_data.KnockBackFlip, _data.KnockBackPower, _col);
            return;
        }


    }


    /// <summary>
    /// 指定した方向に吹っ飛ぶ
    /// </summary>
    private void ConstKnockBack(KnockBackFlipMode _mode, Vector2 _knockBackVec, float _knockBackPower, Collider _col)
    {
        //吹っ飛ぶ方向を決める（右側か左側か）

        float flipVec = 0;

        //基準がない場合はベクトルの正規方向に飛ぶ
        if (_mode == KnockBackFlipMode.None) flipVec = 1;
        //基準がコライダーの場合
        else if (_mode == KnockBackFlipMode.Collider) { flipVec = transform.position.x - _col.WorldPos.x >= 0 ? 1 : -1; }
        //基準が持ち主の場合
        else if (_mode == KnockBackFlipMode.Character)
        {
            var x = _col.WorldPos.x - _col.LocalPos.x;
            flipVec = transform.position.x - x >= 0 ? 1 : -1;
        }



        //反転を加味して吹っ飛ばす
        var vector = _knockBackVec;
        vector.x *= flipVec;



        rb.linearVelocity = vector * _knockBackPower;



    }

    /// <summary>
    /// 放射状に飛ぶ
    /// </summary>
    private void RadialKnockBack(KnockBackFlipMode _mode, float _knockBackPower, Collider _col)
    {

        if (_mode == KnockBackFlipMode.Collider)
        {
            var v = (transform.position - _col.WorldPos).normalized;
            rb.linearVelocity = v * _knockBackPower;
            return;
        }
        else if (_mode == KnockBackFlipMode.Character)
        {
            //Characterならそのコライダーの持ち主の中心点を基準に左右の判定をする
            var x = _col.WorldPos - _col.LocalPos;
            var v = (transform.position - x).normalized;
            rb.linearVelocity = v * _knockBackPower;
            return;
        }


    }


    /// <summary>
    /// Vector2の方向に吹っ飛ぶ
    /// </summary>
    /// <param name="vec"></param>
    public void KnockBack(Vector2 vec)
    {

        rb.linearVelocity = vec;
    }

    /// <summary>
    /// スタンするフレーム数をセットする
    /// </summary>
    /// <param name="value"></param>
    public void SetStanFlame(int value)
    {
        stanFrame = value;
    }








    /// <summary>
    /// ジャンプ処理（Animation関係なし、動くだけ）
    /// </summary>
    public void JumpAction()
    {
        var v = rb.linearVelocity;
        v.y = jumpForce;
        rb.linearVelocity = v;

    }


    /// <summary>
    /// シールド回復処理
    /// </summary>
    public void ShieldHeal()
    {
        if (currentShield >= maxShield) return;
        currentShield++;

    } 

    public void ShieldMaxHeal()
    {
        currentShield = maxShield;
    }

    public void ShieldDamage(int damage=1)
    {
        currentShield -=damage;
    }

    /// <summary>
    /// 自身の生成しているHitBoxをすべて削除する
    /// </summary>
    public void HitBoxReset()
    {
        colManager.DestroyCircleCol(hitboxColList);
        hitboxColList = null;
    }

    /// <summary>
    /// 自身の生成しているPushBoxを削除する
    /// </summary>
    public void PushBoxReset()
    {
        colManager.DestroyCircleCol(pushBox);
        pushBox = null;
    }

    /// <summary>
    /// 当たり判定を通常時の場所に生成する
    /// </summary>
    public void HitBoxDefaultSet()
    {
        CircleColData hitbox = new();
        hitbox.localPos = Vector3.zero;
        hitbox.radius = 1f;
        hitbox.trans = gameObject.transform;

        List<CircleColData> hitboxList = new();
        hitboxList.Add(hitbox);

  
        hitboxColList = colManager.UpdateHitBoxColliderData(hitboxList, charaCon);



    }


    /// <summary>
    /// HitBoxを生成する
    /// </summary>
    /// <param name="colDatas"></param>
    public void UpdateHitBoxColliderData(List<CircleColData> colDatas)
    {
        hitboxColList = colManager.UpdateHitBoxColliderData(colDatas, charaCon);
    }


    /// <summary>
    /// 足場に足がついているか確認する
    /// </summary>
    /// <returns></returns>
    public bool GroundCheck()
    {
        CapsuleCollider2D pCol = GetComponent<CapsuleCollider2D>();
        //var pColHeightHalf = transform.localScale.y/2;

        //RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);

        var pColHeightHalf = pCol.size.y / 2;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down * (pColHeightHalf + 0.01f), Vector2.down, 0.1f);



        //rayを飛ばし何もなかったら抜ける
        if (hit.collider == null) return false;

        if (hit.collider.tag == "Player") return false;
        //Debug.Log(hit.collider, gameObject);

        return true;



    }
    public int GetHP() => hp;
    /// <summary>
    /// 現在のシールド割合を取得する
    /// </summary>
    /// <returns></returns>
    public float GetShieldRatio() => (float)currentShield / (float)maxShield;


}

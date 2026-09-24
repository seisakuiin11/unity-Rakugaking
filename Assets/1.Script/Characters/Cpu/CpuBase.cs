using UnityEngine;


/// <summary>
/// CPUのベースクラス 各キャラに派生する
/// </summary>
public class CpuBase
{
    protected CharacterController myCharacter;
    protected CharacterController targetPlayer;

    /// <summary>
    /// ジャンプして敵に届く範囲
    /// </summary>
    protected const float JumpAreaLength = 10.0f;
    /// <summary>
    /// 敵が真横にいると判定する誤差範囲
    /// </summary>
    protected const float VerticalDiff = 0.3f;

    const float AttackUpAreaLengthX = 1.5f;
    const float AttackUpAreaLengthY = 4f;
    const float AttackUpDiffY = 0.5f;
    const float AttackDownAreaLength = 4f;
    const float AttackDownDiffY = -0.5f;
    const float AttackLeftAreaLength = 2.5f;
    const float AttackRightAreaLength = 2.5f;

    protected bool clock;

    // 足場を降りるよう
    bool descendFlag;
    int descendCount;
    const int descendMaxFlame = 15;


    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_character"></param>
    public virtual void Init(CharacterController _character)
    {
        myCharacter = _character;
    }

    /// <summary>
    /// アクションを考える 思考
    /// </summary>
    /// <param name="_characters"></param>
    public virtual InputCommandData Think(CharacterController[] _characters)
    {
        // ターゲットを決める
        SetTarget(_characters, out Vector3 myPos, out Vector3 targetPos);

        var data = new InputCommandData();
        float dis = Vector3.Distance(myPos, targetPos);
        float disX = Mathf.Abs(targetPos.x - myPos.x);
        float disY = targetPos.y - myPos.y;

        // 足場を降りる
        if(Descend(ref data)) return data;

        // ターゲットを向く
        LookTarget(ref data, myPos, targetPos);


        // 判定をするかしないかのフラグ
        clock = !clock;
        // 攻撃判定をしない時
        if (clock) return data;


        // 下に敵がいて、攻撃範囲なら
        if (disY < AttackDownDiffY && dis < AttackDownAreaLength)
        {
            data.ATTACK_DOWN = true;
            return data;
        }

        // 相手が上にいて、攻撃範囲なら
        if (disY >= AttackUpDiffY && disX < AttackUpAreaLengthX && disY < AttackUpAreaLengthY)
        {
            data.ATTACK_UP = true;
            return data;
        }

        // 相手が横にいて、攻撃範囲なら
        if (Mathf.Abs(disY) <= VerticalDiff && disX < AttackLeftAreaLength)
        {
            data.ATTACK_LEFT = true;
            return data;
        }

        // 相手が横にいて、攻撃範囲なら
        if (Mathf.Abs(disY) <= VerticalDiff && disX < AttackRightAreaLength)
        {
            data.ATTACK_RIGHT = true;
            return data;
        }

        // 敵が上にいたら、ジャンプ
        JumpThink(ref data, dis, disY);

        // 敵が真下にいたら、足場を降りる
        DescendThink(ref data, disY);

        return data;
    }

    /// <summary>
    /// ターゲットを決める
    /// </summary>
    protected void SetTarget(CharacterController[] _characters, out Vector3 myPos, out Vector3 targetPos)
    {
        // 距離判定
        float dis = 100f;
        foreach (var character in _characters)
        {
            if(character == null) continue;

            if (character.GetIsDead() || character == myCharacter) continue;
            
            float _dis = Vector3.Distance(myCharacter.transform.position, character.transform.position);

            if (_dis >= dis) continue;

            dis = _dis;
            targetPlayer = character;
        }

        myPos = myCharacter.transform.position;
        targetPos = targetPlayer.transform.position;
    }

    /// <summary>
    /// ターゲットの方向を向く
    /// </summary>
    protected void LookTarget(ref InputCommandData data, Vector3 myPos, Vector3 targetPos)
    {
        // 敵を向く
        // 右にいるか左にいるか判定する
        if (myPos.x > targetPos.x)
        {
            data.DIRECTION_VEC2 += Vector2.left;
            data.DIRECTION_DATA = DIRECTIONDATA.LEFT;
        }
        else if (myPos.x < targetPos.x)
        {
            data.DIRECTION_VEC2 += Vector2.right;
            data.DIRECTION_DATA = DIRECTIONDATA.RIGHT;
        }
    }

    /// <summary>
    /// ジャンプするか考える
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dis">ターゲットとの距離</param>
    /// <param name="disY">Y軸のターゲットとの距離</param>
    protected void JumpThink(ref InputCommandData data, float dis, float disY)
    {
        // 敵が上にいたら、ジャンプ
        if (disY > 0 && Mathf.Abs(disY) > VerticalDiff && dis < JumpAreaLength)
        {
            data.DIRECTION_VEC2 += Vector2.up;
            data.JUMP = true;
        }
    }

    /// <summary>
    /// 足場を降りるか考える
    /// </summary>
    /// <param name="data"></param>
    /// <param name="disY">Y軸のターゲットとの距離</param>
    protected void DescendThink(ref InputCommandData data, float disY)
    {
        if (disY < 0 && Mathf.Abs(disY) > VerticalDiff && myCharacter.GetCharacterCommon.FootingGroundCheck(out var col))
        {
            descendFlag = true;
            data.DIRECTION_DATA = DIRECTIONDATA.DOWN;
        }
    }

    protected bool Descend(ref InputCommandData data)
    {
        if(!descendFlag) return false;

        descendCount++;
        data.DIRECTION_DATA = DIRECTIONDATA.DOWN;

        if (descendCount >= descendMaxFlame)
        {
            descendFlag = false;
            descendCount = 0;
        }

        return descendFlag;
    }

    /// <summary>
    /// 与えられた値のパーセントのサイコロがtrueかfalseか判断する
    /// </summary>
    /// <param name="_percent"></param>
    /// <returns></returns>
    protected bool CpuMoveRoll(float _percent)
    {
        var roll=UnityEngine.Random.Range(0f, 1f);

        if(roll<_percent)return true;
        return false;

    }
}

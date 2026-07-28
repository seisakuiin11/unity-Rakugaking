using UnityEngine;

public class CpuBase
{
    CharacterController myCharacter;
    CharacterController targetPlayer;

    const float JumpAreaLength = 10.0f;
    const float DaibakuhatuAreaLength = 5.5f;
    const float JinarasiAreaLength = 4.2f;
    const float WashoiAreaLength = 3.0f;
    const float StrikeAreaLength = 7f;
    const float VerticalDiff=0.3f;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_character"></param>
    public void Init(CharacterController _character)
    {
        myCharacter = _character;
    }

    /// <summary>
    /// アクションを考える 思考
    /// </summary>
    /// <param name="_characters"></param>
    public InputCommandData Think(CharacterController[] _characters)
    {
        SetTarget(_characters);
        Vector3 myPos = myCharacter.transform.position;
        Vector3 targetPos = targetPlayer.transform.position;

        var data = new InputCommandData();
        float dis = Vector3.Distance(myPos, targetPos);
        float disX = Mathf.Abs(targetPos.x - myPos.x);
        float disY = targetPos.y - myPos.y;


        


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



        // 一定距離以内で、相手がスタン状態なら、大爆発
        if (targetPlayer.State() == CHARA_STATE.stan && dis < DaibakuhatuAreaLength)
        {
            data.ATTACK_UP_OLD = true;
            data.ATTACK_UP = true;
            return data;
        }

        // 相手が下にいて、自分が空中にいるなら、一定距離以内なら、地ならしを発動
        if(myCharacter.State() == CHARA_STATE.air && dis < JinarasiAreaLength)
        {
            data.ATTACK_DOWN = true;
            return data;
        }

        // ある程度距離を詰められたら、アッパーをカマス
        if (dis < WashoiAreaLength)
        {
            data.ATTACK_LEFT = true;
            return data;
        }

        // 敵が真横にいて、ある程度の距離にいる場合、近づくためにもローリングを行う
        if(Mathf.Abs(disY)<=VerticalDiff && disX < StrikeAreaLength)
        {
            if (targetPlayer.State() == CHARA_STATE.atkDown&& myCharacter.State() != CHARA_STATE.air)
            {
            data.DIRECTION_VEC2 += Vector2.up;
            data.JUMP = true;
            return data;
            }

            data.ATTACK_RIGHT = true;
            return data;
        }

        // 敵が上にいたら、ジャンプ
        if(disY > 0 && dis < JumpAreaLength)
        {
            data.DIRECTION_VEC2 += Vector2.up;
            data.JUMP = true;
        }


        if(disY < 0 && myCharacter.FootingGroundCheck(out var col))
        {
            data.DIRECTION_DATA = DIRECTIONDATA.DOWN; 
            return data;
        }
        return data;
    }

    /// <summary>
    /// ターゲットを決める
    /// </summary>
    void SetTarget(CharacterController[] _characters)
    {
        // 距離判定
        float dis = 100f;
        foreach (var character in _characters)
        {
            if (character.GetIsDead() || character == myCharacter) continue;

            float _dis = Vector3.Distance(myCharacter.transform.position, character.transform.position);

            if (_dis >= dis) continue;

            dis = _dis;
            targetPlayer = character;
        }
    }

    /// <summary>
    /// 与えられた値のパーセントのサイコロがtrueかfalseか判断する
    /// </summary>
    /// <param name="_percent"></param>
    /// <returns></returns>
    private bool CpuMoveRoll(float _percent)
    {
        var roll=UnityEngine.Random.Range(0f, 1f);

        Debug.Log(roll);
        if(roll<_percent)return true;
        return false;

    }
}

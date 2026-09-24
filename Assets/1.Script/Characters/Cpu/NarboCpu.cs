using UnityEngine;

/// <summary>
/// ナルボ CPU
/// </summary>
public class NarboCpu : CpuBase
{
    const float DaibakuhatuAreaLength = 6f;
    const float JinarasiAreaLength = 3.6f;
    const float WashoiAreaLength = 3.0f;
    const float StrikeAreaLength = 7f;


    /// <summary>
    /// アクションを考える 思考
    /// </summary>
    /// <param name="_characters"></param>
    public override InputCommandData Think(CharacterController[] _characters)
    {
        // ターゲットを決める
        SetTarget(_characters, out Vector3 myPos, out Vector3 targetPos);

        var data = new InputCommandData();
        float dis = Vector3.Distance(myPos, targetPos);
        float disX = Mathf.Abs(targetPos.x - myPos.x);
        float disY = targetPos.y - myPos.y;


        // 足場を降りる
        if (Descend(ref data)) return data;

        // ターゲットを向く
        LookTarget(ref data, myPos, targetPos);

        // 相手(ナルボ)がある行動をしていたら
        // 相手がだいばくはつをやっていたら
        if (targetPlayer.State() == CHARA_STATE.atkUp && dis < DaibakuhatuAreaLength)
        {
            data.DIRECTION_VEC2 = Vector2.zero;
            data.DIRECTION_DATA = DIRECTIONDATA.Neutral;
            LookTarget(ref data, targetPos, myPos);
            return data;
        }

        // 判定をするかしないかのフラグ
        clock = !clock;
        // 攻撃判定をしない時
        if (clock) return data;


        // 一定距離以内で、相手がスタン状態なら、大爆発
        if (targetPlayer.State() == CHARA_STATE.stan && dis < DaibakuhatuAreaLength)
        {
            data.ATTACK_UP = true;
            return data;
        }

        // 相手が下にいて、自分が空中にいるなら、一定距離以内なら、地ならしを発動
        if (myCharacter.State() == CHARA_STATE.air && disX < JinarasiAreaLength && disY < 0f)
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
        if (Mathf.Abs(disY) <= VerticalDiff && disX < StrikeAreaLength)
        {
            if (targetPlayer.State() == CHARA_STATE.atkDown && myCharacter.State() != CHARA_STATE.air)
            {
                data.DIRECTION_VEC2 += Vector2.up;
                data.JUMP = true;
                return data;
            }

            // 60%の確率
            if (CpuMoveRoll(0.6f))
            {
                data.ATTACK_RIGHT = true;
                return data;
            }
            else {
                // 地ならしの距離なら ジャンプ
                if(dis < JinarasiAreaLength)
                {
                    data.ATTACK_DOWN = true;
                    return data;
                }
                else
                {
                    data.DIRECTION_VEC2 += Vector2.up;
                    data.JUMP = true;
                    return data;
                }
            }
        }

        // 敵が上にいたら、ジャンプ
        JumpThink(ref data, dis, disY);

        // 敵が真下にいたら、足場を降りる
        DescendThink(ref data, disY);

        return data;
    }
}

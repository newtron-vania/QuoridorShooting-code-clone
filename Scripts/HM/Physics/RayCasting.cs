using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HM
{
    namespace Physics
    {
        public static class RayUtils
        {
            // 이동의 가능 여부를 물리 계산을 통해 확인하는 함수
            public static bool[] CheckWallRay(Vector3 start, Vector2 direction) // return [외벽 여부, 벽 여부, 적과의 충돌이 있는지]
            {
                RaycastHit2D outerWallHit = Physics2D.Raycast(start, direction.normalized, GameManager.GridSize * direction.magnitude, LayerMask.GetMask("OuterWall")); // 외벽에 의해 완전히 막힘
                RaycastHit2D wallHit = Physics2D.Raycast(start, direction.normalized, GameManager.GridSize * direction.magnitude, LayerMask.GetMask("Wall")); // 벽에 의해 완전히 막힘
                RaycastHit2D[] semiWallHit = Physics2D.RaycastAll(start, direction.normalized, GameManager.GridSize * direction.magnitude, LayerMask.GetMask("SemiWall")); // 벽에 의해 "반" 막힘
                RaycastHit2D[] tokenHit = Physics2D.RaycastAll(start, direction.normalized, GameManager.GridSize * direction.magnitude, LayerMask.GetMask("Token")).Where(hit2D => hit2D.transform.position != start).OrderBy(h => h.distance).ToArray(); // 적에 의해 완전히 막힘

                bool fullBlock = false;
                if (outerWallHit)
                {
                    return new bool[] { true, false, tokenHit.Length > 0 };
                }
                if (!wallHit)
                { // 벽에 의해 완전히 막히지 않았고
                    for (int j = 0; j < semiWallHit.Length; j++)
                    { // 반벽이 2개가 겹쳐있을 경우에
                        for (int k = j + 1; k < semiWallHit.Length; k++)
                        {
                            float wallDistance = Mathf.Abs(semiWallHit[j].distance - semiWallHit[k].distance);
                            if (wallDistance > 0.1f) continue;
                            if (semiWallHit[j].transform.rotation == semiWallHit[k].transform.rotation || Mathf.Abs(semiWallHit[j].distance - semiWallHit[k].distance) < 0.000001f)
                            {
                                fullBlock = true; // 완전 막힘으로 처리
                                break;
                            }
                        }
                        if (fullBlock) break;
                    }
                    if (!fullBlock)
                    { // 완전 막히지 않았고 적이 공격 범주에 있다면 공격한다.
                        return new bool[] { false, false, tokenHit.Length > 0 };
                    }
                }
                return new bool[] { false, true, tokenHit.Length > 0 };
            }
        }
    }
}
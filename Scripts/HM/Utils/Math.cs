using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HM
{
    namespace Utils
    {
        public static class MathUtils
        {
            // 두 점 사이의 택시 거리(맨해튼 거리)
            public static float GetManhattaDistance(Vector2Int start, Vector2Int end)
            {
                return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
            }
        }
    }
}
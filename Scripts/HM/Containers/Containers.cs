using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


namespace HM
{
    namespace Containers
    {
        // [LEGACY CODE
        public class Define
        {
            public enum EField
            {
                None = -1,
                Player,
                Enemy,
            }
        }
        // /LEGACY CODE]

        public class Path
        {
            // 중앙 좌표상 (0, 0) 시작으로 x, y 좌표
            // G = 시작으로부터 이동한 거리, H = 가로, 세로로 벽을 무시하고 Player까지 이동한 거리
            public float G, H;
            public float F => G + H;
            public Vector2Int Position;
            public Path(int x, int y)
            {
                Position = new Vector2Int(x, y);
            }

            public Path(Vector2Int pos)
            {
                Position = pos;
            }

            public Path ParentPath;
        }
        // [LEGACY CODE
        public class EnemyValue
        {
            private Vector3 worldPosition; // position
            private int mMoveCtrl; // moveCtrl

            public int hp; // 유닛 hp
            public int maxHp; // 유닛 최대 hp
            public int attack;
            public int damageResistance;
            public int moveCtrl
            {
                get
                {
                    return mMoveCtrl;
                }

                set
                {
                    // Debug.Log($"SetPreMoveCtrl : {index}: {value}");
                    value = Mathf.Max(value, 0);
                    // Debug.Log($"SetMoveCtrl : {index}: {value}");
                    // Enemy correctEnemy = EnemyManager.GetEnemy(mPosition);
                    // correctEnemy.moveCtrl[1] = value;
                    mMoveCtrl = value;
                }
            }
            public int maxMoveCtrl; // 유닛이 가질 수 있는 최대 행동력
            public int uniqueNum; // 어떤 유닛을 생성할지 정하는 번호
            public int Index; // 생성 순서, EnemyBox 내 Index
            public Vector2Int Position // position이 변경될때 일어나는 것
            {
                get
                {
                    return GameManager.ToGridPosition(worldPosition);
                }

                set
                {
                    worldPosition = GameManager.ToWorldPosition(value);
                    //TODO: 적의 실제 위치를 변경하는 코드
                }
            }

            public EnemyValue(int hp, int moveCtrl, int uniqueNum, int index, Vector3 position)
            {
                this.hp = hp;
                mMoveCtrl = moveCtrl;
                this.uniqueNum = uniqueNum;
                this.Index = index;
                worldPosition = position;
            }
        } // /LEGACY CODE]
        /*

        public class PlayerValues
        {
            public GameObject player; // 해당 게임 오브젝트
            public int hp; // 체력 최소값 10, 최댓값 100
            public int attack; // 최소값 1, 최댓값 50
            public int damageResistance; // 0%, 51%
            public int index; // 해당 캐릭터 고유번호

            public int mMoveCtrl; // 행동력
            public int moveCtrl // 프로퍼티
            {
                get
                {
                    return mMoveCtrl;
                }

                set
                {

                }
            }

            public Vector3 mPosition; // 해당 위치
            public Vector3 position
            {
                get
                {
                    return mPosition;
                }

                set
                {
                    player.transform.position = position;
                    mPosition = value;
                }
            }

            public PlayerValues(int hp, int moveCtrl, int index, Vector3 position)
            {
                this.hp = hp;
                mMoveCtrl = moveCtrl;
                this.index = index;
                mPosition = position;
            }
        }*/
    }
}
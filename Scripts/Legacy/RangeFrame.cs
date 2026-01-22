using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterDefinition;

namespace Legacy
{
    using CharacterDefinition.Legacy;
    [System.Serializable]
    public class RangeSetting
    {
        [Header("Total Property Value")]
        public List<Vector2Int> range;
    }

    public class RangeFrame : MonoBehaviour
    {
        // Rnage를 인스펙터창에서 설정 후 인덱스 값에 따라 정해진 Vector2Int 리턴할 수 있게 만듦 2차원 리스트형태
        [SerializeField] private List<RangeSetting> _moveProperties = new List<RangeSetting>();
        [SerializeField] private List<RangeSetting> _attackProperties = new List<RangeSetting>();
        [SerializeField] private List<RangeSetting> _skillProperties = new List<RangeSetting>();

        // 원하는 field 선택 후 해당 field에 번호 입력
        public List<Vector2Int> SelectFieldProperty(PlayerRangeField field = PlayerRangeField.None, int Property = 0)
        {
            RangeSetting currentField = null;
            switch (field)
            {
                case PlayerRangeField.Move:
                    currentField = _moveProperties[Property];
                    break;
                case PlayerRangeField.Attack:
                    currentField = _attackProperties[Property];
                    break;
                case PlayerRangeField.Skill:
                    currentField = _skillProperties[Property];
                    break;
                default:
                    Debug.LogError("아무런 필드값을 받지 못했습니다. AxisFrame.cs");
                    break;
            }

            return currentField.range;
        }
    }
}

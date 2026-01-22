using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

/* 
 * 데이터 변수명들은 시트에 있는 그대로 또는, 기존에 사용하던 변수명으로 해두었습니다.
 * 캐릭터, 스킬 쪽 데이터는 수정이 필요한 변수명들이 많아, 확정되어 전달받는대로 수정할 예정입니다.
*/



// 플레이어 정보
[Serializable]
public class PlayerInfo
{
    public UnlockCharacter UnlockCharacter;
}

// 잠금해제된 캐릭터 정보
[Serializable]
public class UnlockCharacter
{
    public List<int> CharacterIdList;
}

[Serializable]
// 파티 구성 정보
public class Party
{
    public int PartyMaxCompositionNum;             // 파티 구성 최대인원수
    public List<int> PartyMembers;                 // 파티 구성원 (캐릭터 ID를 저장)
}


[Serializable]
public class RangeData
{
    public int Id;
    public List<Vector2Int> Range;

    public RangeData(int id, List<Vector2Int> range)
    {
        this.Id = id;
        this.Range = range;
    }
}

[Serializable]
public class RangeDatas
{
    public List<RangeData> Datas;
}

[Serializable]
public class AttackRangeData
{
    public List<RangeData> Datas;
}


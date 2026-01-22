using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public class RelicDatabase : MonoBehaviour
{
    public static RelicDatabase Instance { get; private set; }

    // DataManager에서 사용하는 키와 동일하다고 가정 (상수로 관리 권장)
    private const string RELIC_DATA_PATH = "LegacyData/JSON/RelicTestDatas"; // 확장자 제외
    private const string HEX_KEY = "YourHexKeyHere"; // 실제 키값 필요

    private readonly Dictionary<int, RelicData> _relicLibrary = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRelicData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadRelicData()
    {
        var jsonFile = Resources.Load<TextAsset>(RELIC_DATA_PATH);
        if (jsonFile == null) return;

        try
        {
            // 현재 JSON 구조는 무조건 Wrapper 형식이므로 Wrapper로 바로 파싱 시도
            var wrapper = JsonConvert.DeserializeObject<RelicDataWrapper>(jsonFile.text);
        
            if (wrapper != null && wrapper.Datas != null)
            {
                _relicLibrary.Clear();
                foreach (var relic in wrapper.Datas)
                {
                    _relicLibrary[relic.Id] = relic;
                }
                Debug.Log($"[RelicDatabase] 로드 성공: {_relicLibrary.Count}개");
                
                if (_relicLibrary.Count > 0)
                {
                    // 데이터 저장 로직 후
                    PrintAllRelics(); // 확인을 위한 출력 호출
                }
            }
        }
        catch (JsonSerializationException jsonEx)
        {
            // Enum 이름 불일치 같은 구체적인 파싱 에러를 여기서 잡습니다.
            Debug.LogError($"[RelicDatabase] JSON 데이터 형식 오류: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelicDatabase] 기타 오류: {ex.Message}");
        }
    }

    public RelicData GetRelic(int id)
    {
        if (_relicLibrary.TryGetValue(id, out var data)) return data;
        return null;
    }

    public List<RelicData> GetAllRelics()
    {
        return _relicLibrary.Values.ToList();
    }
    
    
    
    public void PrintAllRelics()
    {
        if (_relicLibrary.Count == 0)
        {
            Debug.LogWarning("[RelicDatabase] 출력할 유물 데이터가 없습니다.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<color=cyan><b>[RelicDatabase] 총 {_relicLibrary.Count}개의 유물 로드 완료</b></color>");
        sb.AppendLine("==================================================");

        foreach (var relic in _relicLibrary.Values)
        {
            sb.AppendLine($"<b>ID:</b> {relic.Id} | <b>Name:</b> {relic.Name}");
            sb.AppendLine($"<b>Rarity:</b> {relic.Rarity} | <b>Target:</b> {relic.TargetFilter}");
            sb.AppendLine($"<b>Description:</b> {relic.Description}");

            // 트리거 타입 리스트 출력
            string triggers = string.Join(", ", relic.TriggerTypes);
            sb.AppendLine($"<b>Triggers:</b> [{triggers}]");

            // 트리거 파라미터 (Dictionary) 출력
            if (relic.TriggerParams != null && relic.TriggerParams.Count > 0)
            {
                string tParams = string.Join(", ", relic.TriggerParams.Select(kv => $"{kv.Key}: {kv.Value}"));
                sb.AppendLine($"   └ TriggerParams: {tParams}");
            }

            // 로직 타입 및 파라미터 출력
            sb.AppendLine($"<b>Logic:</b> {relic.LogicType}");
            if (relic.LogicParams != null && relic.LogicParams.Count > 0)
            {
                string lParams = string.Join(", ", relic.LogicParams.Select(kv => $"{kv.Key}: {kv.Value}"));
                sb.AppendLine($"   └ LogicParams: {lParams}");
            }

            sb.AppendLine($"<b>IconPath:</b> {relic.IconPath}");
            sb.AppendLine("--------------------------------------------------");
        }

        Debug.Log(sb.ToString());
    }
}

[Serializable]
public class RelicDataWrapper
{
    // [중요] JSON 파일의 키 값("Datas")과 매핑
    [JsonProperty("Datas")] public List<RelicData> Datas;
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class RelicDatabase : MonoBehaviour
{
    public static RelicDatabase Instance { get; private set; }

    private const string RELIC_DATA_PATH = "LegacyData/JSON/RelicTestDatas";

    private readonly Dictionary<int, RelicData> _relicLibrary = new();

    // JSON 직렬화 설정 — StringEnumConverter로 enum 문자열 파싱 지원
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter()
        },
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
    };

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
            var wrapper = JsonConvert.DeserializeObject<RelicDataWrapper>(jsonFile.text, _jsonSettings);

            if (wrapper?.Datas != null)
            {
                _relicLibrary.Clear();
                foreach (var relic in wrapper.Datas)
                {
                    _relicLibrary[relic.Id] = relic;
                }
                Debug.Log($"[RelicDatabase] 로드 성공: {_relicLibrary.Count}개");

                if (_relicLibrary.Count > 0)
                    PrintAllRelics();
            }
        }
        catch (JsonSerializationException jsonEx)
        {
            Debug.LogError($"[RelicDatabase] JSON 데이터 형식 오류: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelicDatabase] 기타 오류: {ex.Message}");
        }
    }

    public RelicData GetRelic(int id)
    {
        return _relicLibrary.TryGetValue(id, out var data) ? data : null;
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
            sb.AppendLine($"<b>Rarity:</b> {relic.Rarity} | <b>Target:</b> {relic.TargetFilter ?? "N/A"}");
            sb.AppendLine($"<b>Description:</b> {relic.Description}");

            // 트리거 타입
            if (relic.TriggerTypes != null)
            {
                string triggers = string.Join(", ", relic.TriggerTypes);
                sb.AppendLine($"<b>Triggers:</b> [{triggers}]");
            }

            // 트리거 파라미터 (JToken)
            if (relic.TriggerParams != null && relic.TriggerParams.Count > 0)
            {
                string tParams = string.Join(", ", relic.TriggerParams.Select(kv => $"{kv.Key}: {kv.Value}"));
                sb.AppendLine($"   └ TriggerParams: {tParams}");
            }

            // 시너지
            if (relic.Synergy != null && relic.Synergy.Count > 0)
            {
                sb.AppendLine($"<b>Synergy:</b> [{string.Join(", ", relic.Synergy)}]");
            }

            // Priority
            if (relic.Priority != 0)
            {
                sb.AppendLine($"<b>Priority:</b> {relic.Priority}");
            }

            // Effects (신규 스키마)
            if (relic.Effects != null && relic.Effects.Count > 0)
            {
                sb.AppendLine($"<b>Effects:</b> ({relic.Effects.Count}개)");
                for (int i = 0; i < relic.Effects.Count; i++)
                {
                    var effect = relic.Effects[i];
                    sb.Append($"   [{i}] {effect.EffectType}");
                    if (effect.Condition != null)
                        sb.Append($" (조건: {effect.Condition.ConditionType})");
                    if (effect.IsReversible)
                        sb.Append(" [가역]");
                    sb.AppendLine();
                }
            }

            // 하위호환: LogicType (Phase 1)
            if (relic.LogicType.HasValue)
            {
                sb.AppendLine($"<b>LogicType (Legacy):</b> {relic.LogicType.Value}");
                if (relic.LogicParams != null && relic.LogicParams.Count > 0)
                {
                    string lParams = string.Join(", ", relic.LogicParams.Select(kv => $"{kv.Key}: {kv.Value}"));
                    sb.AppendLine($"   └ LogicParams: {lParams}");
                }
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
    [JsonProperty("Datas")] public List<RelicData> Datas;
}

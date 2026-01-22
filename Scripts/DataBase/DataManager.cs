
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;

    private const string CHARACTER_DATA_PATH = "Data/character";
    private const string SUPPLYMENT_DATA_PATH = "Data/supply";
    private const string SKILL_DATA_PATH = "Data/skill";
    private const string ARTIFACT_DATA_PATH = "Data/artifact";

    private const string STATUS_EFFECT_DATA_PATH = "Data/status_effect";
    private const string CREATURE_DATA_PATH = "Data/creature";

    private const string RANGE_DATA_PATH = "Data/range";


    // 플레이어 정보 Json 위치
    private string _playerInfoJsonPath;

    // 파티 정보 Json 위치
    private string _partyDataJsonPath;

    [SerializeField]
    private Dictionary<int, CharacterData> _characterData = new Dictionary<int, CharacterData>();
    [SerializeField]
    private Dictionary<int, CharacterData> _playableCharacters = new Dictionary<int, CharacterData>();

    [SerializeField]
    private Dictionary<int, CharacterData> _enemyCharacters = new Dictionary<int, CharacterData>();

    [SerializeField]
    private Dictionary<int, SkillData> _skillData = new Dictionary<int, SkillData>();

    [SerializeField]
    private Dictionary<int, SupplymentData> _supplymentData = new Dictionary<int, SupplymentData>();
    [SerializeField]
    private Dictionary<int, EffectData> _effects = new Dictionary<int, EffectData>();
    [SerializeField]
    private Dictionary<int, StatuseffectData> _statusEffectData = new Dictionary<int, StatuseffectData>();

    [SerializeField]
    private Dictionary<int, List<Vector2Int>> _rangeData = new Dictionary<int, List<Vector2Int>>();

    [SerializeField]
    private PlayerInfo _playerInfo = new PlayerInfo();

    [SerializeField]
    private Party _party = new Party();

    // 프로퍼티
    public int PlayableCharacterCount => 9;//_playableCharacters.Count;
    public int EnemyCharacterCount => 4;
    public int PartyMaxComposiitionNum => _party.PartyMaxCompositionNum;
    public int SupplyDatasCount => _supplymentData.Count;
    public int RangesCount => _rangeData.Count;

    public int SkillDataCount => _skillData.Count;

    public int EffectDataCount => _effects.Count;

    public int StatuseffectDataCount => _statusEffectData.Count;

    private const string HEX_KEY = "3502079b5457365b06e5c44fe7468d0a7672beeedd273437a6b9152575088381";


    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject().AddComponent<DataManager>();
                _instance.name = nameof(DataManager);
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        LoadData();
    }
    void Update()
    {

    }
    private void Start()
    {
        // 파티 최대인원 임의 설정.
        _party.PartyMaxCompositionNum = 3;
    }

    // 데이터 읽어와 저장해두기
    private void LoadData()
    {
        try
        {
            _characterData = DataLoader.Load<List<CharacterData>>(Resources.Load<TextAsset>(CHARACTER_DATA_PATH).bytes, HEX_KEY).ToDictionary(character => character.Index);
            _rangeData = DataLoader.Load<Dictionary<int, List<Vector2Int>>>(Resources.Load<TextAsset>(RANGE_DATA_PATH).bytes, HEX_KEY);
            _skillData = DataLoader.Load<List<SkillData>>(Resources.Load<TextAsset>(SKILL_DATA_PATH).bytes, HEX_KEY).ToDictionary(skill => skill.Id);
            _supplymentData = DataLoader.Load<List<SupplymentData>>(Resources.Load<TextAsset>(SUPPLYMENT_DATA_PATH).bytes, HEX_KEY).ToDictionary(supply => supply.Id);
            _statusEffectData = DataLoader.Load<List<StatuseffectData>>(Resources.Load<TextAsset>(STATUS_EFFECT_DATA_PATH).bytes, HEX_KEY).ToDictionary(status => status.Id);

            foreach (var range in _rangeData)
            {
                if (range.Value.Count == 0)
                {
                    for (int x = -8; x <= 8; x++)
                    {
                        for (int y = -8; y <= 8; y++)
                        {
                            range.Value.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }

            Debug.Log("[Info] DataManager::LoadData - Data loaded successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] DataManager::LoadData - Error loading data: {ex.Message}");
            return;
        }
    }

    // 캐릭터 잠금 해제
    // public void LockCharacter(int characterNum, bool isLock)
    // {
    //     if (isLock)
    //     {
    //         _playerInfo.UnlockCharacter.CharacterIdList.Add(characterNum);
    //     }
    //     else
    //     {
    //         _playerInfo.UnlockCharacter.CharacterIdList.Remove(characterNum);
    //     }

    //     SaveJsonData(_playerInfo, _playerInfoJsonPath);
    // }

    // 입력받은 번호의 플레이어블 캐릭터 정보 반환
    public CharacterData GetPlayableCharacterInfo(int characterID)
    {
        return _characterData[characterID];
    }

    // 입력받은 번호의 적 캐릭터 정보 반환
    public CharacterData GetEnemyCharacterInfo(int characterID)
    {
        return _characterData[characterID];
    }

    // 특수능력 정보 반환
    public SkillData GetSkillData(int skillId)
    {
        return _skillData[skillId];
    }

    // 보급품 정보 반환
    public SupplymentData GetSupplyData(int supplyId)
    {
        return _supplymentData[supplyId];
    }

    // 효과 정보 반환
    public EffectData GetEffectData(int effectId)
    {
        return _effects[effectId];
    }

    public StatuseffectData GetStatuseffectData(int statuseffectID)
    {
        return _statusEffectData[statuseffectID];
    }

    public List<Vector2Int> GetRangeData(int id, bool isPlayableCharacter = true)
    {
        // -1번은 모든 칸 이동/공격 가능 (GameManager의 GetFullRange() 사용하면 해당 캐릭터의 위치를 기준으로 보드 위 모든 좌표 반환)
        if (id == -1)
        {
            return null;
        }

        // List<Vector2Int> ranges = new List<Vector2Int>(_rangeDatas[id]);
        List<Vector2Int> ranges = _rangeData[id];

        if (!isPlayableCharacter)
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                ranges[i] *= -1;
            }
        }

        return ranges;
    }
}

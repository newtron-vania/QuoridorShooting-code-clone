using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DG.Tweening;
using HM;
using HM.Utils;
using System.IO;
using System.Reflection;
using CharacterDefinition;

public class SupplyManager : MonoBehaviour, IEventListener
{
    public static SupplyManager _instance;

    private const string SUPPLY_BOX_PREFAB_PATH = "Supply/SupplyBoxPrefab";
    private const string SUPPLY_DROP_PREFAB_PATH = "Supply/SupplyPrefab";

    private int _selectedSupplyItem = 0;

    // 보급품 획득 후 3가지 중 선택된 아이디 1개
    public int SelectedSupplyItem
    {
        set
        {
            _selectedSupplyItem = value;
            if (SupplyShowPanelUI != null)
            {
                SupplyShowPanelUI.SupplyItemsReset();
            }
        }
        get
        {
            return _selectedSupplyItem;
        }
    }
    public SupplyShowPanelUI SupplyShowPanelUI = null;
    // public CharacterController SupplyCharacterController;
    public BaseSupply CurrentUseBaseSupply;

    // 테스트 용 public 나중에 private로 수정할 예정 + GameManger쪽으로 옮길 예정
    public Dictionary<int, int> _supplyInventory = new Dictionary<int, int>();
    public List<BaseSupply> _saveDurationSupply = new List<BaseSupply>();
    public SupplymentData.SupplyTarget _supplyTarget = SupplymentData.SupplyTarget.None;
    public int _currentUseSupply = -1;
    // 미리 캐싱
    public Dictionary<int, System.Type> _supplyDataType = new Dictionary<int, System.Type>();

    // 인벤에 있는 보급품의 총량 계산
    public int SupplyInventoryValueCount
    {
        get
        {
            int count = 0;
            foreach (int value in _supplyInventory.Values)
            {
                count += value;
            }
            return count;
        }
    }

    public static SupplyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject().AddComponent<SupplyManager>();
                _instance.name = nameof(SupplyManager);
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
    public static void Reset()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }
    private void Awake()
    {
        _supplyInventory.Add(9, 2);
        //SupplyCaching();
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        EventManager.Instance.AddEvent(HM.EventType.OnTurnStart, this);
        EventManager.Instance.AddEvent(HM.EventType.OnTurnEnd, this);
    }

    public void OnEvent(HM.EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case HM.EventType.OnTurnStart:
                CheckSupplyQueue();
                break;

            case HM.EventType.OnTurnEnd:
                CheckSupplyQueue();
                break;

            //case HM.EventType.OnRoundEnd:
            //    ResetSupplyInventory();
            //    break;
        }
    }

    // 스테이지 초기화 시 인벤토리 리셋 (해당 부분 기획서 수정으로 리셋 타이밍 변경 예정)
    //public void ResetSupplyInventory()
    //{
    //    _supplyInventory.Clear();
    //}

    public void SupplyCaching()
    {
        for(int i = 1; i < DataManager.Instance.SupplyDatasCount; i++)
        {
            string supplyName = DataManager.Instance.GetSupplyData(i).EditName;

            Assembly assembly = Assembly.GetExecutingAssembly();
            _supplyDataType.Add(i, assembly.GetType(supplyName));
        }
    }

    // 적이 죽었을 때 죽은 자리에 보급품 생성
    public void DropSupplyToEnemy(Transform enemyPos)
    {
        GameObject dropSupply = ResourceManager.Instance.Instantiate(SUPPLY_DROP_PREFAB_PATH);
        dropSupply.tag = "DropSupply"; // 태그 결정
        dropSupply.AddComponent<SupplyAttach>(); // 보급품 아이템에 SupplyAttach 부착 미리 해당 보급품에 마주쳤을 때 얻을 보급품 결정
        dropSupply.transform.position = new Vector3(enemyPos.position.x, enemyPos.position.y, 0);
    }

    // 인수 1 : 보물상자 소환 갯수
    public void SpawnSupplyBox(int spawnCount)
    {
        Vector2Int spawnPos;
        List<Vector2Int> spawnPoses = new List<Vector2Int>();
        for (int i = 0; i < spawnCount; i++)
        {
            do
            {
                spawnPos = new Vector2Int(Random.Range(-4, 5), Random.Range(-4, 5));
            } while (CanSpawnSupplyBox(spawnPos) || spawnPoses.Contains(spawnPos)); // 타일위에 아군, 적 기물이 있는지 확인

            // 보물상자 설치
            spawnPoses.Add(spawnPos); // 보물상자 위치 저장
            GameObject spawnObject = ResourceManager.Instance.Instantiate(SUPPLY_BOX_PREFAB_PATH); // 보물상자 소환
            spawnObject.tag = "BoxSupply";
            spawnObject.AddComponent<SupplyAttach>();
            spawnObject.transform.position = GameManager.ToWorldPosition(spawnPos); // 보물상자 위치 조정
        }
    }

    // ID 번호를 받아 기술 String를 이용해 인스턴스화 실시
    public void UseSupplyItem(int id)
    {
        string supplyName = DataManager.Instance.GetSupplyData(id).EditName;

        object obj = System.Activator.CreateInstance(_supplyDataType[id]);

        if (obj == null)
        {
            Debug.LogError($"[ERROR] SupplyManger::UseSupplyItem - 해당 보급품 스크립트가 존재하지않습니다.{supplyName}");
            return;
        }
        CurrentUseBaseSupply = (obj as BaseSupply);
        CurrentUseBaseSupply.InitSupply();
    }

    // 매 턴이 시작됐을 때 한번씩 실행되는 함수
    // 사용된 지속이 필요한 보급품을 한번씩 실행시키고 업데이트
    public void CheckSupplyQueue(SupplymentData.SupplyTarget target = SupplymentData.SupplyTarget.None)
    {
        // 보급품에 아무것도 없으면 예외처리
        if (_saveDurationSupply.Count == 0) return;

        List<BaseSupply> delSupplyItem = new List<BaseSupply>();
        foreach (BaseSupply child in _saveDurationSupply)
        {
            // 턴마다 적용되어야하는 보급품 목록들 업데이트 후 조건 확인
            if (!child.UpdateSupply())
            {
                delSupplyItem.Add(child);
                Debug.Log($"[INFO]SupplyManager(CheckSupplyQueue) - {child.Name} 사용이 끝났습니다.");
            }
        }

        // 사용이 끝난 보급품 리스트에서 삭제
        foreach (BaseSupply child in delSupplyItem)
        {
            _saveDurationSupply.Remove(child);
        }
    }

    // 피해감소에 관련된 Supply 사용시 확인
    public int CheckDefenseSupply(GameObject targetToken)
    {
        Debug.Log($"[INFO]SupplyManager(CheckDefenseSupply) - {targetToken.name}");
        if (_saveDurationSupply.Count == 0) return 0;

        int amount = 0;
        foreach (BaseSupply child in _saveDurationSupply)
        {
            if (child.ID == 11)
            {
                amount = child.EffectAmount;
            }
            else if (child.Type == SupplymentData.SupplyType.Defense)
            {
                if (child.ID != 10 && child.SupplyCharacterStat.Index == int.Parse(targetToken.name.Split("_")[1]))
                {
                    amount = child.EffectAmount;
                }
            }
        }
        return amount;
    }

    // 보급품을 사용할 수 있는지 확인
    public bool CanUseSupply(int ID, int targetID)
    {
        bool returnValue = true;
        BaseSupply saveSupply = null;
        foreach (BaseSupply child in _saveDurationSupply)
        {
            if (child.ID == ID && child.SupplyCharacterStat.Index == targetID)
            {
                // 중복 사용시 이전 지속시간 갱신
                saveSupply = child;
                returnValue = false;
            }
        }
        // 기존에 있던 보급품 효과를 제거하고 새로운 보급품으로 덮어씌우기
        if (!returnValue)
        {
            _saveDurationSupply.Remove(saveSupply);
            returnValue = true;
        }
        return returnValue;
    }

    private bool CanSpawnSupplyBox(Vector2Int pos)
    {
        // 해당 위치에 적, 아군 기물이 있을경우
        if (GameManager.Instance.CharacterController.GetObjectToPosition(GameManager.ToWorldPosition(pos)))
        {
            return true;
        }
        // 해당 위치에 보급품이 있는 경우
        else if(Physics2D.OverlapBox(pos, new Vector2(1,1), 0, LayerMask.GetMask("Supply")))
        {
            return true;
        }
        // 스킬, 보급품, 유물로 인한 추가 소환 위치 조정이 필요할 경우 조건을 추가할 것

        return false;
    }
}

// 보급품 데이터를 사용해 cs파일 생성
public class CreateSupplyClass
{
    public static string SupplyClassFileDirectory = "Assets/Scripts/SupplySystem/Supplys";
    public static string SupplyClassDataFilePath = "Assets/Resources/CSV/SupplyDatas";
    public static string SupplyClassTemplateFilePath = SupplyClassFileDirectory + "/Template.txt";
    public static string T = "########";
    public static string TemplateString = "";
    private static string[] _lines;

    public static BaseSupply[] SupplyInfo;

    // [MenuItem("Assets/Create SupplyClassFile")]
    static void CreateSupplyClassFiles()
    {
        if (Directory.Exists(SupplyClassFileDirectory) == false)
        {
            Debug.LogWarning("[WARN] SupplyManger::CreateSupplyClassFiles - 해당 디렉토리가 존재하지않습니다.");
            Directory.CreateDirectory(SupplyClassFileDirectory);
        }

        if (File.Exists(SupplyClassDataFilePath + ".csv") == true)
        {
            TextAsset textAsset = Resources.Load<TextAsset>("CSV/SupplyDatas");

            string data = textAsset.text;

            _lines = data.Split('\n');
        }
        else
        {
            Debug.LogWarning("[WARN] SupplyManger::CreateSupplyClassFiles - 해당 보급품 데이터 파일이 존재하지 않습니다.");
            return;
        }

        if (File.Exists(SupplyClassTemplateFilePath) == true)
        {
            TemplateString = File.ReadAllText(SupplyClassTemplateFilePath);

            if (TemplateString.Length == 0)
            {
                Debug.LogWarning("[WARN] SupplyManger::CreateSupplyClassFiles - 템플릿에 내용이 비어있습니다.");
                return;
            }

            // CSV위 카테고리 string 배열 생성
            string[] changeString = _lines[0].Split(',');

            // 파일 생성
            for (int i = 1; i < _lines.Length - 1; i++)
            {
                string[] row = _lines[i].Split(',');

                string temp = TemplateString;
                temp = temp.Replace(T, row[10]);

                string newSupplyFilePath = SupplyClassFileDirectory + "/" + row[10] + ".cs";
                if (File.Exists(newSupplyFilePath) == true)
                {
                    Debug.LogWarning($"[WARN] SupplyManger::CreateSupplyClassFiles - 해당 파일은 이미 존재합니다. : {row[10]}.cs");
                }
                else
                {
                    // 비고 제외 카테고리에 해당하는 문자열 전부 데이터 값으로 변경
                    for (int j = 0; j < changeString.Length - 1; j++)
                    {
                        string text = ChangeStringToEnum(changeString[j], row[j]);
                        temp = temp.Replace(changeString[j], text);
                    }

                    File.WriteAllText(newSupplyFilePath, temp);
                }
            }
        }
        else
        {
            Debug.LogWarning("[WARN] SupplyManger::CreateSupplyClassFiles - 해당 템플릿이 존재하지 않습니다.");
            return;
        }
    }

    public static string ChangeStringToEnum(string key, string value)
    {
        decimal number = 0;
        if (key.Equals("sp_grade")) return ((SupplymentData.SupplyGrade)int.Parse(value)).ToString();
        else if (key.Equals("sp_type")) return ((SupplymentData.SupplyType)int.Parse(value)).ToString();
        else if (key.Equals("sp_target")) return ((SupplymentData.SupplyTarget)int.Parse(value)).ToString();
        else return decimal.TryParse(value, out number) ? value : $"\"{value}\"";
    }
}

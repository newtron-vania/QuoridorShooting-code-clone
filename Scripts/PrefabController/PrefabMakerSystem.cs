
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum PrefabType
{
    SimpleCircle,
    SimpleBox,
    SimpleWall,
    GlowBox,
    SkillArea,
}

public enum MakerType
{
    MovePreview,
    AttackPreview,
    BlockedPreview,
    HighlightPreview,
    HighlightMovePreview,
    CharacterPositionPreview,
    RedAttackPreview,
    EnemyCharacterPositionPreview,
    SkillPreview,
    SkillRangePreview,
    Wall,
    SkillArea_Normal,
    SkillArea_Rotten,
    SkillArea_TimeDistortion,
}

public class PrefabMakerSystem : MonoBehaviour
{
    //해당 씬에서 사용할 타입 지정
    [SerializeField]
    private List<MakerType> _makerPreInitList = new List<MakerType>();

    public Dictionary<PrefabType, PrefabPool<PrefabControllerBase>> ObjectPoolDict { get; set; } = new Dictionary<PrefabType, PrefabPool<PrefabControllerBase>>();

    public Dictionary<MakerType, PrefabObjectMaker> ObjectMakerDict { get; set; } = new Dictionary<MakerType, PrefabObjectMaker>();

    public static PrefabMakerSystem _instance;


    private Dictionary<string,Sprite> _spriteDict = new Dictionary<string, Sprite>();


    public static PrefabMakerSystem Instance
    {
        get 
        {
            if(_instance == null)
            {
                Debug.LogWarning("[Warning] PrefabMakerSystem::Instance - 현재 씬에서 기본 설정 안하고 사용 중");
                GameObject container = new GameObject(nameof(PrefabMakerSystem));
                _instance = container.AddComponent<PrefabMakerSystem>();
            }
            return _instance;
        }

    }

    public void Awake()
    {
        _instance = this;
        Init();
    }

    public void OnDestroy()
    {
        _instance = null;
    }

    public void Init()
    {
        //상남자는 스트링키 사용함 이미지 이름이랑 같게 권장
        _spriteDict["Square"]= ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");
        _spriteDict["BlockSquare"] = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/BlockSquare");

        foreach (var type in _makerPreInitList)
        {
            InitObjectMaker(type);
        }
    }

    private void InitPrefabPool(PrefabType prefabType)
    {
        GameObject prefab = null;
        int preCreateCount = 15;
        switch (prefabType)
        {
            case PrefabType.SimpleCircle:
                prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Simple/SimpleCirclePrefab");
                break;
            case PrefabType.SimpleBox:
                prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Simple/SimpleBoxPrefab");
                break;
            case PrefabType.GlowBox:
                prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Simple/GlowBoxPrefab");
                break;
            case PrefabType.SimpleWall:
                prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/PlayerWall");
                break;
            case PrefabType.SkillArea:
                prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Skill/SkillAreaPrefab");
                break;

        }

        PrefabPool<PrefabControllerBase> prefabPool = new PrefabPool<PrefabControllerBase>(prefab, transform);
        prefabPool.CreatePrefabs(preCreateCount);
        ObjectPoolDict[prefabType] = prefabPool;

    }

    private PrefabPool<PrefabControllerBase> GetPrefabPool(PrefabType prefabType)
    {
        if (!ObjectPoolDict.ContainsKey(prefabType))
            InitPrefabPool(prefabType);
        return ObjectPoolDict[prefabType];
    }

    //switch 직접 작성
    private void InitObjectMaker(MakerType makerType)
    {
        PrefabObjectMaker objectMaker = null;
        Sprite img;
        PrefabPool<PrefabControllerBase> pool;
        Material mat;


        //태그 삑사리 조심!!!!
        switch (makerType)
        {
            case MakerType.MovePreview:

                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, new Color(0.278f, 0.714f, 1f, 0.4f),"PlayerPreview");

                break;
            case MakerType.AttackPreview:

                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, new Color(0.278f, 0.714f, 1f, 0.4f),"PlayerAttackPreview");
                break;
            case MakerType.RedAttackPreview:

                img = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, new Color(0.9372549f, 0.3254902f, 0.3137255f, 0.6f), "PlayerAttackPreview");
                break;
            case MakerType.BlockedPreview:

                img = _spriteDict["BlockSquare"];

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new BlockedBoxPreviewMaker(pool, img);
                break;
            case MakerType.HighlightPreview:
                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, Color.cyan, "PlayerAttackPreview");
                break;
            case MakerType.Wall:
                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SimpleWall);

                objectMaker = new WallMaker(pool, img);
                break;
            case MakerType.CharacterPositionPreview:
                img = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");

                mat = ResourceManager.Instance.Load<Material>("Material/Preview/GrowPreview");

                pool = GetPrefabPool(PrefabType.GlowBox);

                objectMaker = new GlowPreviewMaker(pool, img, mat ,Color.cyan ,"CharacterPositionPreview", new Color(1, 1, 1));
                break;
            case MakerType.EnemyCharacterPositionPreview:
                img = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");

                mat = ResourceManager.Instance.Load<Material>("Material/Preview/EnemyGrowPreview");

                pool = GetPrefabPool(PrefabType.GlowBox);

                objectMaker = new GlowPreviewMaker(pool, img, mat, new Color(0.1960784f, 0.9098039f, 0.0627451f, 0.7f), "CharacterPositionPreview", new Color(0.05882353f, 0.3686275f, 0));
                break;
            case MakerType.SkillPreview:
                img = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, new Color(0.8666667f, 0.8f, 1), "SkillPreview"); // 연한 보라색 (#DECCFF)
                break;
            case MakerType.SkillRangePreview:
                img = ResourceManager.Instance.Load<Sprite>("Sprites/Preview/Square");

                pool = GetPrefabPool(PrefabType.SimpleBox);

                objectMaker = new SimplePreviewMaker(pool, img, new Color(0.8666667f, 0.8f, 1), "SkillRangePreview"); // 연한 보라색 (#DECCFF)
                break;

            case MakerType.SkillArea_Normal:
                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SkillArea);

                objectMaker = new SkillAreaMaker(pool, img,Color.clear);
                break;
            case MakerType.SkillArea_Rotten:
                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SkillArea);

                objectMaker = new SkillAreaMaker(pool, img, new Color(0.8f, 0.95f, 0.0f, 0.8f));
                break;
            case MakerType.SkillArea_TimeDistortion:
                img = _spriteDict["Square"];

                pool = GetPrefabPool(PrefabType.SkillArea);

                objectMaker = new SkillAreaMaker(pool, img, Color.cyan);
                break;
            default:


                break;
        }

        ObjectMakerDict[makerType] = objectMaker;
    }


    public PrefabObjectMaker GetObjectMaker(MakerType makerType)
    {
        if (!ObjectMakerDict.ContainsKey(makerType))
            InitObjectMaker(makerType);

        return ObjectMakerDict[makerType];
    }
}

using CharacterDefinition;
using UnityEngine;

public partial class BaseCharacter : IEffectableProvider, ISkillParticipant, IStatuseffectParticipant
{

    public T GetEffectable<T>() where T :  IEffectable
    {
        if (this is T effectable)
            return effectable;
        else
        {
            Debug.LogError("[Error] BaseCharcter::GetEffectable: 해당 객체엔" + typeof(T).Name + "인터페이스가 없음!!");
            return default(T);
        }
    }

    public StatuseffectController StatuseffectController { get; set; } = new();

    //적,플레이어 캐릭터에 대한 고유id가 필요해서 추가 작업 //걍 캐릭터컨트롤러에서 아예 이렇게 해주면 편할텐데... identification말고
    public int BattleId
    {
        get
        {
            int id = Id;
            if (!Playerable)
                id += 1 << 8;
            return id;
        }
    }

    public int SkillId => SkillIndex;
    public CharacterStat CharacterStat => characterStat;

    protected SkillSystem _skillSystem;

    public CharacterIdentification Identification
    {
        get
        {
            if (Playerable)
                return CharacterIdentification.Player;
            return CharacterIdentification.Enemy;
        }
    }

    public void AttachSkillSystem(SkillSystem skillSystem)
    {
        _skillSystem = skillSystem;
        skillSystem.BindCharacterEvent(this);

    }
    
    public void SlipForward(Vector2Int prevPos, int value)
    {
        Vector2Int forward = Position - prevPos;
        int slipX = (forward.x == 0) ? 0 : (int)Mathf.Sign(forward.x) * value;
        int slipY = (forward.y == 0) ? 0 : (int)Mathf.Sign(forward.y) * value;

        bool[] wallChecker = HM.Physics.RayUtils.CheckWallRay(GameManager.ToWorldPosition(Position), new Vector2Int(slipX, slipY));

        Vector2Int slipPosition = new Vector2Int(Position.x + slipX, Position.y + slipY);



        if ((wallChecker[0] || wallChecker[1]))
            return;

        //캐릭터는 CheckWallRay로 못 잡아냄
        foreach (Vector2Int characterPos in Controller.GetAllCharactersPosition())
        {
            if (characterPos == slipPosition)
                return;
        }

        Position = slipPosition;

    }


    public int TakeSkillDamage(BaseCharacter baseCharacter, int damage = 0)
    {
        if (damage == 0)
            return 0;

        UIManager.Instance.InputLogEvent(LogEvent.SkillUse, baseCharacter.CharacterObject.gameObject);
        return TakeDamage(this, damage);
    }




}

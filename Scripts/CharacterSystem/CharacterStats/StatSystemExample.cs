using UnityEngine;

public class StatSystemExample : MonoBehaviour
{
    void Start()
    {
        Example1_BasicModifier();
        Example2_MultipleModifiers();
        Example3_RemoveModifier();
        Example4_GlobalModifier();
        Example5_OnRemoveCallback();
        Example6_RemovalStrategies();
        Example7_HpSyncWithMaxHp();
        Example8_ConditionalModifiers();
        Example9_TurnBasedModifiers();
    }

    void Example1_BasicModifier()
    {
        BaseStat baseStat = new BaseStat(1, "Warrior", 1000, 1000, 100, 0.2f, 100, 20);
        CharacterStat characterStat = new CharacterStat(baseStat);

        Debug.Log($"[INFO] StatSystemExample::Example1_BasicModifier() - 초기 MaxHP: {characterStat.MaxHp}");

        StatModifier hpBuff = new StatModifier(
            "hp_buff_300",
            StatType.MaxHp,
            StatModifierType.FlatBaseAdd,
            300
        );
        characterStat.AddModifier(hpBuff);

        Debug.Log($"[INFO] StatSystemExample::Example1_BasicModifier() - 버프 후 MaxHP: {characterStat.MaxHp}");
    }

    void Example2_MultipleModifiers()
    {
        BaseStat baseStat = new BaseStat(2, "Mage", 800, 800, 150, 0.1f, 100, 20);
        CharacterStat characterStat = new CharacterStat(baseStat);

        Debug.Log($"[INFO] StatSystemExample::Example2_MultipleModifiers() - 초기 공격력: {characterStat.Atk}");

        StatModifier flatBonus = new StatModifier(
            "atk_flat_20",
            StatType.Atk,
            StatModifierType.FlatBaseAdd,
            20,
            0
        );

        StatModifier percentBonus = new StatModifier(
            "atk_percent_15",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.15f,
            1
        );

        characterStat.AddModifier(percentBonus);
        characterStat.AddModifier(flatBonus);

        Debug.Log($"[INFO] StatSystemExample::Example2_MultipleModifiers() - 버프 후 공격력: {characterStat.Atk}");
    }

    void Example3_RemoveModifier()
    {
        BaseStat baseStat = new BaseStat(3, "Tank", 1500, 1500, 80, 0.3f, 100, 20);
        CharacterStat characterStat = new CharacterStat(baseStat);

        characterStat.Hp = 1200;
        Debug.Log($"[INFO] StatSystemExample::Example3_RemoveModifier() - 현재 HP: {characterStat.Hp}/{characterStat.MaxHp}");

        StatModifier hpBuff = new StatModifier(
            "temp_hp_buff",
            StatType.MaxHp,
            StatModifierType.FlatBaseAdd,
            300
        );
        characterStat.AddModifier(hpBuff);

        Debug.Log($"[INFO] StatSystemExample::Example3_RemoveModifier() - 버프 적용: {characterStat.Hp}/{characterStat.MaxHp}");

        characterStat.RemoveModifier("temp_hp_buff");

        Debug.Log($"[INFO] StatSystemExample::Example3_RemoveModifier() - 버프 해제: {characterStat.Hp}/{characterStat.MaxHp}");
    }

    void Example4_GlobalModifier()
    {
        StatManager manager = StatManager.Instance;

        BaseStat baseStat1 = new BaseStat(10, "Hero1", 1000, 1000, 100, 0.2f, 100, 20);
        BaseStat baseStat2 = new BaseStat(11, "Hero2", 900, 900, 120, 0.15f, 100, 20);

        CharacterStat char1 = new CharacterStat(baseStat1);
        CharacterStat char2 = new CharacterStat(baseStat2);

        manager.RegisterCharacter(10, char1);
        manager.RegisterCharacter(11, char2);

        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - Hero1 공격력: {char1.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - Hero2 공격력: {char2.Atk}");

        StatModifier globalBuff = new StatModifier(
            "global_atk_buff",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.2f
        );
        manager.AddModifierToAll(globalBuff);

        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - 전역 버프 후 Hero1: {char1.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - 전역 버프 후 Hero2: {char2.Atk}");

        manager.RemoveModifierFromAll("global_atk_buff");

        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - 버프 제거 후 Hero1: {char1.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example4_GlobalModifier() - 버프 제거 후 Hero2: {char2.Atk}");
    }

    void Example5_OnRemoveCallback()
    {
        BaseStat baseStat = new BaseStat(4, "Rogue", 800, 800, 150, 0.1f, 100, 20);
        CharacterStat characterStat = new CharacterStat(baseStat);

        Debug.Log($"[INFO] StatSystemExample::Example5_OnRemoveCallback() - 초기 공격력: {characterStat.Atk}");

        bool buffActive = true;
        StatModifier timedBuff = new StatModifier(
            "timed_atk_buff",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.5f,
            0,
            onRemove: () =>
            {
                buffActive = false;
                Debug.Log("[INFO] StatSystemExample::Example5_OnRemoveCallback() - 시간제 공격력 버프가 해제되었습니다!");
            }
        );

        characterStat.AddModifier(timedBuff);
        Debug.Log($"[INFO] StatSystemExample::Example5_OnRemoveCallback() - 버프 적용: {characterStat.Atk}, 활성: {buffActive}");

        characterStat.RemoveModifier("timed_atk_buff");
        Debug.Log($"[INFO] StatSystemExample::Example5_OnRemoveCallback() - 버프 해제: {characterStat.Atk}, 활성: {buffActive}");
    }

    void Example6_RemovalStrategies()
    {
        Debug.Log("[INFO] StatSystemExample::Example6_RemovalStrategies() - === 옵션 1: 비율 유지 전략 (기본) ===");
        BaseStat baseStat1 = new BaseStat(5, "Knight", 1500, 1500, 100, 0.3f, 100, 20);
        CharacterStat char1 = new CharacterStat(baseStat1);
        char1.Hp = 1200;

        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 초기 HP: {char1.Hp}/{char1.MaxHp}");

        StatModifier hpBuff1 = new StatModifier("hp_buff", StatType.MaxHp, StatModifierType.FlatBaseAdd, 300);
        char1.AddModifier(hpBuff1);
        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 버프 적용: {char1.Hp}/{char1.MaxHp}");

        char1.RemoveModifier("hp_buff");
        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 버프 해제 (비율 유지): {char1.Hp}/{char1.MaxHp}");

        Debug.Log("[INFO] StatSystemExample::Example6_RemovalStrategies() - === 옵션 2: 조정 없음 전략 ===");
        BaseStat baseStat2 = new BaseStat(6, "Paladin", 1500, 1500, 100, 0.3f, 100, 20);
        CharacterStat char2 = new CharacterStat(baseStat2, new NoAdjustmentStrategy());
        char2.Hp = 1200;

        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 초기 HP: {char2.Hp}/{char2.MaxHp}");

        StatModifier hpBuff2 = new StatModifier("hp_buff2", StatType.MaxHp, StatModifierType.FlatBaseAdd, 300);
        char2.AddModifier(hpBuff2);
        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 버프 적용: {char2.Hp}/{char2.MaxHp}");

        char2.RemoveModifier("hp_buff2");
        Debug.Log($"[INFO] StatSystemExample::Example6_RemovalStrategies() - 버프 해제 (조정 없음): {char2.Hp}/{char2.MaxHp}");
    }

    void Example7_HpSyncWithMaxHp()
    {
        Debug.Log("[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - === MaxHp 변동 시 Hp 동기화 종합 테스트 ===");

        // 초기 스탯: MaxHp=1000, Hp=1000
        BaseStat baseStat = new BaseStat(7, "TestHero", 1000, 1000, 100, 0.2f, 100, 20);
        CharacterStat character = new CharacterStat(baseStat);

        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [초기 상태] Hp={character.Hp}, MaxHp={character.MaxHp}");

        // Hp - 100
        character.Hp = character.Hp - 100;
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Hp -100] Hp={character.Hp}, MaxHp={character.MaxHp}");

        // 4개의 MaxHp Modifier 추가
        Debug.Log("[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - --- 4개의 MaxHp Modifier 추가 ---");

        StatModifier mod1 = new StatModifier("mod1_flat_base", StatType.MaxHp, StatModifierType.FlatBaseAdd, 200);
        character.AddModifier(mod1);
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 1 추가: FlatBaseAdd +200] Hp={character.Hp}, MaxHp={character.MaxHp}");

        StatModifier mod2 = new StatModifier("mod2_percent_base", StatType.MaxHp, StatModifierType.PercentBaseAdd, 0.5f);
        character.AddModifier(mod2);
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 2 추가: PercentBaseAdd +50%] Hp={character.Hp}, MaxHp={character.MaxHp}");

        StatModifier mod3 = new StatModifier("mod3_flat_current", StatType.MaxHp, StatModifierType.FlatCurrentAdd, 300);
        character.AddModifier(mod3);
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 3 추가: FlatCurrentAdd +300] Hp={character.Hp}, MaxHp={character.MaxHp}");

        StatModifier mod4 = new StatModifier("mod4_percent_current", StatType.MaxHp, StatModifierType.PercentCurrentAdd, 0.2f);
        character.AddModifier(mod4);
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 4 추가: PercentCurrentAdd +20%] Hp={character.Hp}, MaxHp={character.MaxHp}");

        // Hp를 150이 될 때까지 깎기
        Debug.Log("[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - --- Hp를 150으로 감소 ---");
        character.Hp = 150;
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Hp 150으로 설정] Hp={character.Hp}, MaxHp={character.MaxHp}");

        // 4개의 Modifier를 하나씩 제거
        Debug.Log("[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - --- Modifier 제거 (역순) ---");

        character.RemoveModifier("mod4_percent_current");
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 4 제거: PercentCurrentAdd +20%] Hp={character.Hp}, MaxHp={character.MaxHp}");

        character.RemoveModifier("mod3_flat_current");
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 3 제거: FlatCurrentAdd +300] Hp={character.Hp}, MaxHp={character.MaxHp}");

        character.RemoveModifier("mod2_percent_base");
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 2 제거: PercentBaseAdd +50%] Hp={character.Hp}, MaxHp={character.MaxHp}");

        character.RemoveModifier("mod1_flat_base");
        Debug.Log($"[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - [Modifier 1 제거: FlatBaseAdd +200] Hp={character.Hp}, MaxHp={character.MaxHp}");

        Debug.Log("[INFO] StatSystemExample::Example7_HpSyncWithMaxHp() - === 테스트 완료 ===");
    }

    void Example8_ConditionalModifiers()
    {
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - === 조건부 Modifier 및 ModifierGroup 테스트 ===");

        // 초기 스탯
        BaseStat baseStat = new BaseStat(11, "Berserker", 1000, 1000, 100, 0.2f, 100, 20);
        CharacterStat character = new CharacterStat(baseStat);

        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - [초기 상태] Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        // 조건부 ModifierGroup 생성: HP > 20일 때 매턴 HP -20 (최소 20), 공격력 +50%
        var berserkerGroup = new ModifierGroup(
            "berserker_mode",
            canRepeat: true, // 매턴 반복 가능
            onApply: () => Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - [Berserker Mode 발동!]"),
            onRemove: () => Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - [Berserker Mode 해제!]")
        );

        // 그룹 조건: HP > 20
        berserkerGroup.Condition += (character) => character.Hp > 20;

        // HP 감소 Modifier (FlatCurrentAdd -20)
        var hpReduction = new ConditionalStatModifier(
            "berserker_hp_drain",
            StatType.MaxHp,
            StatModifierType.FlatCurrentAdd,
            -20
        );
        // 개별 Modifier 조건: 항상 적용
        hpReduction.Condition += (character) => true;

        // 공격력 증가 Modifier (PercentBaseAdd +50%)
        var atkBoost = new ConditionalStatModifier(
            "berserker_atk_boost",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.5f
        );
        atkBoost.Condition += (character) => true;

        berserkerGroup.AddModifier(hpReduction);
        berserkerGroup.AddModifier(atkBoost);

        character.RegisterModifierGroup(berserkerGroup);

        // 턴 1: HP > 20이므로 발동
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - --- 턴 1 ---");
        character.TryApplyModifierGroup("berserker_mode");
        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        // 턴 2: 그룹 초기화 후 다시 발동 (canRepeat=true)
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - --- 턴 2 ---");
        berserkerGroup.ResetTrigger();
        character.TryApplyModifierGroup("berserker_mode");
        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        // HP를 20으로 강제 설정
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - --- HP를 20으로 강제 설정 ---");
        character.Hp = 20;
        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        // 턴 3: HP = 20이므로 조건 불만족, 발동 안 됨
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - --- 턴 3 (HP=20, 조건 불만족) ---");
        berserkerGroup.ResetTrigger();
        character.TryApplyModifierGroup("berserker_mode");
        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        // ModifierGroup 제거
        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - --- Berserker Mode 제거 ---");
        character.UnregisterModifierGroup("berserker_mode");
        Debug.Log($"[INFO] StatSystemExample::Example8_ConditionalModifiers() - Hp={character.Hp}, MaxHp={character.MaxHp}, Atk={character.Atk}");

        Debug.Log("[INFO] StatSystemExample::Example8_ConditionalModifiers() - === 조건부 테스트 완료 ===");
    }

    void Example9_TurnBasedModifiers()
    {
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - === 턴 기반 Modifier 시스템 테스트 ===");

        StatManager manager = StatManager.Instance;

        // 테스트용 캐릭터 2명 생성
        BaseStat baseStat1 = new BaseStat(20, "Warrior", 1000, 1000, 100, 0.2f, 100, 20);
        BaseStat baseStat2 = new BaseStat(21, "Mage", 800, 800, 150, 0.1f, 100, 20);

        CharacterStat warrior = new CharacterStat(baseStat1);
        CharacterStat mage = new CharacterStat(baseStat2);

        manager.RegisterCharacter(20, warrior);
        manager.RegisterCharacter(21, mage);

        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [초기 상태] Warrior HP={warrior.Hp}/{warrior.MaxHp}, ATK={warrior.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [초기 상태] Mage HP={mage.Hp}/{mage.MaxHp}, ATK={mage.Atk}");

        // 테스트 1: 단일 캐릭터에 3턴 지속 버프 추가
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 테스트 1: Warrior에게 3턴 공격력 버프 ---");
        StatModifier atkBuff = new StatModifier(
            "warrior_atk_buff",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.5f // +50%
        );
        manager.AddModifierWithDuration(20, atkBuff, 3);
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [버프 적용] Warrior ATK={warrior.Atk}, 남은 턴={manager.GetRemainingTurns(20, "warrior_atk_buff")}");

        // 테스트 2: 모든 캐릭터에 2턴 HP 버프 추가
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 테스트 2: 모든 캐릭터에 2턴 HP 버프 ---");
        StatModifier hpBuff = new StatModifier(
            "global_hp_buff",
            StatType.MaxHp,
            StatModifierType.FlatBaseAdd,
            200
        );
        manager.AddModifierToAllWithDuration(hpBuff, 2);
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [전역 버프] Warrior HP={warrior.Hp}/{warrior.MaxHp}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [전역 버프] Mage HP={mage.Hp}/{mage.MaxHp}");

        // 테스트 3: ModifierGroup을 2턴 지속으로 추가
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 테스트 3: Mage에게 2턴 지속 버프 그룹 ---");
        var mageBuffGroup = new ModifierGroup(
            "mage_buff_group",
            canRepeat: false,
            onApply: () => Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [Mage 버프 그룹 발동!]"),
            onRemove: () => Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [Mage 버프 그룹 해제!]")
        );

        var mageAtkMulBoost = new ConditionalStatModifier(
            "mage_group_atk",
            StatType.Atk,
            StatModifierType.PercentBaseAdd,
            0.3f
        );
        mageAtkMulBoost.Condition += (character) => true;

        var mageAtkFlatBoost = new ConditionalStatModifier(
            "mage_group_speed",
            StatType.Atk,
            StatModifierType.FlatBaseAdd,
            10
        );
        mageAtkFlatBoost.Condition += (character) => true;

        mageBuffGroup.AddModifier(mageAtkMulBoost);
        mageBuffGroup.AddModifier(mageAtkFlatBoost);

        manager.AddModifierGroupWithDuration(21, mageBuffGroup, 2);
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [그룹 버프] Mage ATK={mage.Atk}");

        // 턴 시뮬레이션
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 턴 1 종료 시뮬레이션 ---");
        manager.ProcessTurnEnd();
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - Warrior ATK 버프 남은 턴: {manager.GetRemainingTurns(20, "warrior_atk_buff")}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - Global HP 버프 남은 턴: {manager.GetRemainingTurns(20, "global_hp_buff")}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - Mage 그룹 버프 남은 턴: {manager.GetRemainingTurns(21, "mage_buff_group")}");

        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 턴 2 종료 시뮬레이션 ---");
        manager.ProcessTurnEnd();
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [턴 2 종료] Warrior HP={warrior.Hp}/{warrior.MaxHp}, ATK={warrior.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [턴 2 종료] Mage HP={mage.Hp}/{mage.MaxHp}, ATK={mage.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - Warrior ATK 버프 남은 턴: {manager.GetRemainingTurns(20, "warrior_atk_buff")}");

        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 턴 3 종료 시뮬레이션 ---");
        manager.ProcessTurnEnd();
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [턴 3 종료] Warrior HP={warrior.Hp}/{warrior.MaxHp}, ATK={warrior.Atk}");
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [턴 3 종료] Mage HP={mage.Hp}/{mage.MaxHp}, ATK={mage.Atk}");

        // 테스트 4: 무한 지속 버프 (duration = -1)
        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 테스트 4: 무한 지속 버프 ---");
        StatModifier permanentBuff = new StatModifier(
            "permanent_def_buff",
            StatType.Avd,
            StatModifierType.FlatBaseAdd,
            0.1f // +10%
        );
        manager.AddModifierWithDuration(20, permanentBuff, -1); // 무한 지속
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [무한 버프] Warrior Defense={warrior.Avd}");

        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - --- 턴 4 종료 (무한 버프는 유지됨) ---");
        manager.ProcessTurnEnd();
        Debug.Log($"[INFO] StatSystemExample::Example9_TurnBasedModifiers() - [턴 4 종료] Warrior Defense={warrior.Avd} (무한 버프 유지)");

        // 정리
        manager.UnregisterCharacter(20);
        manager.UnregisterCharacter(21);

        Debug.Log("[INFO] StatSystemExample::Example9_TurnBasedModifiers() - === 턴 기반 Modifier 테스트 완료 ===");
    }
}
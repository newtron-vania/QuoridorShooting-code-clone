using System;

public class CharacterStatBuilder
{
    private int _index;
    private string _name;
    private int _maxHp;
    private int _atk;
    private float _avd;
    private int _baseAP;
    private int _apPerTurn;

    public CharacterStatBuilder SetIndex(int index)
    {
        _index = index;
        return this;
    }

    public CharacterStatBuilder SetName(string name)
    {
        _name = name;
        return this;
    }

    public CharacterStatBuilder SetMaxHp(int maxHp)
    {
        _maxHp = maxHp;
        return this;
    }

    public CharacterStatBuilder SetAtk(int atk)
    {
        _atk = atk;
        return this;
    }

    public CharacterStatBuilder SetAvd(float avd)
    {
        _avd = avd;
        return this;
    }

    public CharacterStatBuilder SetBaseAp(int baseAP)
    {
        _baseAP = baseAP;
        return this;
    }

    public CharacterStatBuilder SetApRecovery(int apPerTurn)
    {
        _apPerTurn = apPerTurn;
        return this;
    }

    public CharacterStat Build()
    {
        var baseStat = new BaseStat(_index, _name, _maxHp, _maxHp, _atk, _avd, _baseAP, _apPerTurn);
        return new CharacterStat(baseStat);
    }
}
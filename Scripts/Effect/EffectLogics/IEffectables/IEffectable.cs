
//이펙트 효과를 받는 인터페이스 명시용
public interface IEffectable
{

}

public interface IEffectableProvider
{
    public T GetEffectable<T>() where T : IEffectable;
}



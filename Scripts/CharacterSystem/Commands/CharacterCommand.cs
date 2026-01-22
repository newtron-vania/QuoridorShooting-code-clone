


public abstract class CharacterCommand : ICommand
{
    public BaseCharacter CommandedCharacter;


    public abstract bool ExecuteSuccess();

    public abstract bool Execute();
    public abstract bool Undo();
}

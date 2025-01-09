using System;

public class DataContainer
{
    public static DataContainer Instance => _instance ??= new DataContainer();
    public int SelectedBlockId { get; set; } = -1;
    public float CollapseProbability { get; set; } = 1.0f;

    private static DataContainer _instance = null;
    private Action _initialize = null;
    private Action _gameFinish = null;

    public void InitializeRegister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _initialize += action;
        }
    }

    public void InitializeUnregister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _initialize -= action;
        }
    }

    public void InitializeInvoke() => _initialize.Invoke();

    public void GameFinishRegister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _gameFinish += action;
        }
    }

    public void GameFinishUnregister(params Action[] actions)
    {
        foreach (var action in actions)
        {
            if (action == null) continue;
            _gameFinish -= action;
        }
    }

    public void GameFinishInvoke() => _gameFinish.Invoke();
}

namespace task17;

public class Scheduler : IScheduler
{
    private readonly Queue<ICommand> _commands = new();
    private readonly object _syncRoot = new();

    public bool HasCommand()
    {
        lock (_syncRoot)
        {
            return _commands.Count > 0;
        }
    }

    public ICommand? Select()
    {
        lock (_syncRoot)
        {
            return _commands.Count == 0 ? null : _commands.Dequeue();
        }
    }

    public void Add(ICommand cmd)
    {
        lock (_syncRoot)
        {
            _commands.Enqueue(cmd);
        }
    }
}

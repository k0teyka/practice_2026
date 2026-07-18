namespace task17;

public enum ServerState
{
    Running,
    StoppingHard,
    StoppingSoft
}

public class ServerThread
{
    private readonly Queue<ICommand> _queue = new();
    private Thread? _thread;
    private ServerState _state = ServerState.Running;
    private readonly Action<Exception, ICommand> _exceptionHandler;

    public bool IsCurrentThread => Thread.CurrentThread == _thread;
    public bool IsAlive => _thread?.IsAlive ?? false;

    public ServerThread(Action<Exception, ICommand>? errorHandler = null)
    {
        _exceptionHandler = errorHandler ?? ((_, _) => { });
    }

    public void Start()
    {
        if (_thread != null) return;
        
        _thread = new Thread(ProcessQueue);
        _thread.Start();
    }

    public void Enqueue(ICommand command)
    {
        lock (_queue)
        {
            if (_state == ServerState.Running)
            {
                _queue.Enqueue(command);
                Monitor.Pulse(_queue);
            }
        }
    }

    private void ProcessQueue()
    {
        while (true)
        {
            ICommand? command = null;

            lock (_queue)
            {
                while (_queue.Count == 0 && _state == ServerState.Running)
                {
                    Monitor.Wait(_queue);
                }

                if (_state == ServerState.StoppingHard)
                    break;

                if (_state == ServerState.StoppingSoft && _queue.Count == 0)
                    break;

                if (_queue.Count > 0)
                {
                    command = _queue.Dequeue();
                }
            }

            if (command != null)
            {
                try
                {
                    command.Execute();
                }
                catch (Exception ex)
                {
                    _exceptionHandler(ex, command);
                }
            }
        }
    }

    public void StopHard()
    {
        lock (_queue)
        {
            _queue.Clear();
            _state = ServerState.StoppingHard;
            Monitor.Pulse(_queue);
        }
    }

    public void StopSoft()
    {
        lock (_queue)
        {
            _state = ServerState.StoppingSoft;
            Monitor.Pulse(_queue);
        }
    }

    public void Join()
    {
        _thread?.Join();
    }
}

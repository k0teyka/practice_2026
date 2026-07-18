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
    private readonly IScheduler? _scheduler;
    private Thread? _thread;
    private ServerState _state = ServerState.Running;
    private readonly Action<Exception, ICommand> _exceptionHandler;

    public bool IsCurrentThread => Thread.CurrentThread == _thread;
    public bool IsAlive => _thread?.IsAlive ?? false;

    // Конструктор по умолчанию без планировщика
    public ServerThread(Action<Exception, ICommand>? errorHandler = null)
    {
        _exceptionHandler = errorHandler ?? ((_, _) => { });
    }

    // Конструктор со встроенным планировщиком задач
    public ServerThread(IScheduler scheduler, Action<Exception, ICommand>? errorHandler = null) 
        : this(errorHandler)
    {
        _scheduler = scheduler;
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

            // Если планировщик содержит задачи, берем сначала их (неблокирующий режим)
            if (_scheduler != null && _scheduler.HasCommand())
            {
                command = _scheduler.Select();
            }
            else
            {
                lock (_queue)
                {
                    while (_queue.Count == 0 && _state == ServerState.Running)
                    {
                        // Если в планировщике внезапно появилась задача, не спим
                        if (_scheduler != null && _scheduler.HasCommand()) break;

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
            }

            if (command != null)
            {
                try
                {
                    command.Execute();

                    // Если это длительная задача и она не завершена, возвращаем её в планировщик
                    if (_scheduler != null && command is ILongCommand longCommand && !longCommand.IsCompleted)
                    {
                        _scheduler.Add(command);
                    }
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

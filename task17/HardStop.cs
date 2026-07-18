namespace task17;

public class HardStopCommand : ICommand
{
    private readonly ServerThread _server;

    public HardStopCommand(ServerThread server)
    {
        _server = server;
    }

    public void Execute()
    {
        if (!_server.IsCurrentThread)
        {
            throw new InvalidOperationException("HardStop выполняется только внутри рабочего потока сервера");
        }
        _server.StopHard();
    }
}

namespace task17;

public class SoftStopCommand : ICommand
{
    private readonly ServerThread _server;

    public SoftStopCommand(ServerThread server)
    {
        _server = server;
    }

    public void Execute()
    {
        if (!_server.IsCurrentThread)
        {
            throw new InvalidOperationException("SoftStop выполняется только внутри рабочего потока сервера");
        }
        _server.StopSoft();
    }
}

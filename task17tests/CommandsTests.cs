using task17;

namespace task17tests;

public class TestCommand : ICommand
{
    private readonly Action _action;

    public TestCommand(Action action) 
    { 
        _action = action; 
    }

    public void Execute() 
    { 
        _action(); 
    }
}

public class ServerThreadTests
{
    [Fact]
    public void SoftStopWorkTest()
    {
        var server = new ServerThread();
        server.Start();
        var res = new List<int>();

        server.Enqueue(new TestCommand(() => res.Add(1)));
        server.Enqueue(new TestCommand(() => res.Add(2)));
        server.Enqueue(new TestCommand(() => res.Add(3)));
        server.Enqueue(new SoftStopCommand(server));
        
        server.Join();

        Assert.Equal(3, res.Count);
    }

    [Fact]
    public void HardStopWorkTest()
    {
        var server = new ServerThread();
        server.Start();
        var res = new List<int>();

        server.Enqueue(new TestCommand(() => res.Add(1)));
        server.Enqueue(new HardStopCommand(server));
        server.Enqueue(new TestCommand(() => res.Add(2)));
        
        server.Join();

        Assert.Single(res);
        Assert.Equal(1, res[0]);
        Assert.DoesNotContain(2, res);
    }

    [Fact]
    public void HardStopThrowsExceptionWhenCalledOutside()
    {
        var server = new ServerThread();
        server.Start();
        var command = new HardStopCommand(server);

        var exception = Assert.Throws<InvalidOperationException>(() => command.Execute());
        Assert.Contains("HardStop выполняется только внутри рабочего потока сервера", exception.Message);

        server.Enqueue(new SoftStopCommand(server));
        server.Join();
    }

    [Fact]
    public void SoftStopThrowsExceptionWhenCalledOutside()
    {
        var server = new ServerThread();
        server.Start();
        var command = new SoftStopCommand(server);

        var exception = Assert.Throws<InvalidOperationException>(() => command.Execute());
        Assert.Contains("SoftStop выполняется только внутри рабочего потока сервера", exception.Message);

        server.Enqueue(new SoftStopCommand(server));
        server.Join();
    }

    [Fact]
    public void ThreadWaitsForCommandsCorrectly()
    {
        var server = new ServerThread();
        server.Start();
        
        Thread.Sleep(200);
        Assert.True(server.IsAlive);

        server.Enqueue(new SoftStopCommand(server));
        server.Join();
    }
}

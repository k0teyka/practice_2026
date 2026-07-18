using task17;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace task17tests;

public class LongCommand : ILongCommand
{
    private int _currentStep;
    private readonly int _maxSteps;
    private readonly Action<int>? _stepAction;

    public bool IsCompleted => _currentStep >= _maxSteps;

    public LongCommand(int maxSteps, Action<int>? stepAction = null)
    {
        _maxSteps = maxSteps;
        _stepAction = stepAction;
    }

    public void Execute()
    {
        _currentStep++;
        _stepAction?.Invoke(_currentStep);
    }
}

public class SchedulerTests
{
    [Fact]
    public void SchedulerBasicOperations()
    {
        var scheduler = new Scheduler();
        Assert.False(scheduler.HasCommand());
        Assert.Null(scheduler.Select());

        var command1 = new TestCommand(() => { });
        var command2 = new TestCommand(() => { });

        scheduler.Add(command1);
        scheduler.Add(command2);

        Assert.True(scheduler.HasCommand());
        Assert.Same(command1, scheduler.Select());
        Assert.Same(command2, scheduler.Select());
        Assert.False(scheduler.HasCommand());
    }

    [Fact]
    public void RoundRobinSchedulerTest()
    {
        var scheduler = new Scheduler();
        var executionOrder = new List<string>();

        var command1 = new LongCommand(3, step => executionOrder.Add($"A{step}"));
        var command2 = new LongCommand(3, step => executionOrder.Add($"B{step}"));

        scheduler.Add(command1);
        scheduler.Add(command2);

        while (scheduler.HasCommand())
        {
            var command = scheduler.Select();
            command?.Execute();

            if (command is ILongCommand longcmd && !longcmd.IsCompleted)
            {
                scheduler.Add(command);
            }
        }

        Assert.Equal(6, executionOrder.Count);
        Assert.Equal("A1", executionOrder[0]);
        Assert.Equal("B1", executionOrder[1]);
        Assert.Equal("A2", executionOrder[2]);
        Assert.Equal("B2", executionOrder[3]);
    }

    [Fact]
    public void ServerThreadSchedulerCorrectWork()
    {
        var scheduler = new Scheduler();
        var server = new ServerThread(scheduler);
        var executionLog = new List<string>();

        server.Start();
        scheduler.Add(new LongCommand(2, step => executionLog.Add($"L{step}")));
        server.Enqueue(new TestCommand(() => executionLog.Add("A")));
        server.Enqueue(new TestCommand(() => executionLog.Add("B")));

        Thread.Sleep(200);
        server.Enqueue(new SoftStopCommand(server));
        server.Join();

        Assert.Contains("L1", executionLog);
        Assert.Contains("L2", executionLog);
        Assert.Contains("A", executionLog);
        Assert.Contains("B", executionLog);
    }

    [Fact]
    public void ServerThreadSchedulerExceptionHandler()
    {
        var scheduler = new Scheduler();
        Exception? exception = null;
        ICommand? command = null;

        var server = new ServerThread(scheduler, (ex, cmd) => 
        {
            exception = ex;
            command = cmd;
        });

        server.Start();
        scheduler.Add(new TestCommand(() => throw new InvalidOperationException("TestError")));

        Thread.Sleep(200);
        server.Enqueue(new SoftStopCommand(server));
        server.Join();

        Assert.NotNull(exception);
        Assert.Equal("TestError", exception.Message);
        Assert.NotNull(command);
    }

    [Fact]
    public void GraphicOfSchedulerWork()
    {
        var scheduler = new Scheduler();
        var server = new ServerThread(scheduler);
        var threadProgress = new ConcurrentDictionary<int, List<(double TimeMs, double Progress)>>();
        var startTime = Stopwatch.GetTimestamp();
        
        int[] stepsPeriodThread = { 8, 12, 9 }; 
        
        for (int i = 1; i <= 3; i++)
        {
            threadProgress[i] = new List<(double, double)>();
            int threadId = i;
            int maxSteps = stepsPeriodThread[i - 1];
            
            var command = new LongCommand(maxSteps, step => 
            {
                double currentTime = (Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency * 1000.0;
                double progress = (step / (double)maxSteps) * 100.0;
                
                lock (threadProgress[threadId])
                {
                    threadProgress[threadId].Add((currentTime, progress));
                }
                Thread.Sleep(5); 
            });
            scheduler.Add(command);
        }

        server.Start();
        
        int timeout = 5000; 
        int elapsed = 0;
        while (elapsed < timeout)
        {
            Thread.Sleep(50);
            elapsed += 50;
            
            bool allCompleted = true;
            for (int i = 1; i <= 3; i++)
            {
                if (threadProgress[i].Count < stepsPeriodThread[i - 1])
                {
                    allCompleted = false;
                    break;
                }
            }
            if (allCompleted) break;
        }

        server.Enqueue(new SoftStopCommand(server));
        server.Join();

        // Построение графика через ScottPlot
        var graph = new ScottPlot.Plot();
        var colors = new[] { 
            ScottPlot.Color.FromHex("#0bb084"),
            ScottPlot.Color.FromHex("#bb0000"), 
            ScottPlot.Color.FromHex("#c4199c") 
        };
        var markers = new[] { 
            ScottPlot.MarkerShape.OpenCircle,
            ScottPlot.MarkerShape.FilledSquare,
            ScottPlot.MarkerShape.FilledDiamond
        };

        for (int i = 1; i <= 3; i++)
        {
            var data = threadProgress[i].OrderBy(x => x.TimeMs).ToList();
            double[] xs = data.Select(x => x.TimeMs).ToArray();
            double[] ys = data.Select(x => x.Progress).ToArray();
            
            var sig = graph.Add.SignalXY(xs, ys);
            sig.Color = colors[i - 1];
            sig.LineWidth = 2;
            sig.MarkerSize = 6;
            sig.MarkerShape = markers[i - 1];
            sig.LegendText = $"Поток {i}";
        }

        graph.Title("Работа планировщика задач", 16);
        graph.XLabel("Время выполнения (мс)", 14);
        graph.YLabel("Процент выполнения", 14);
        graph.Axes.AutoScale();
        graph.Legend.IsVisible = true;
        
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string filePath = Path.Combine(path, "GraphicOfSchedulerWork.png");
        graph.SavePng(filePath, 800, 600);
    }
}

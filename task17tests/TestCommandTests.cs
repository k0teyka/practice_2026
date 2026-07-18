using System.Collections.Concurrent;
using System.Diagnostics;
using ScottPlot;
using task17;

namespace task17tests;

public class Task19CombinedTest
{
    [Fact]
    public void FiveCommandsThreeTimes()
    {
        var scheduler = new Scheduler(); 
        var server = new ServerThread(scheduler);
        var progressData = new ConcurrentDictionary<int, List<(double TimeMs, double Progress)>>();
        var sw = Stopwatch.StartNew();
        
        const int commandCount = 5;
        const int stepsPeriodCommand = 3;

        for (int i = 1; i <= commandCount; i++)
        {
            progressData[i] = new List<(double, double)>();
            int id = i;
            
            // Используем уникальное имя класса, чтобы не было конфликта с Action-командой
            var testCmd = new Task19TestCommand(id);
            var repeatedCmd = new RepeatedCommand(testCmd, stepsPeriodCommand);
            var statCmd = new Statistics(repeatedCmd, id, stepsPeriodCommand, 
                (time, prog) => 
                {
                    lock (progressData[id])
                    {
                        progressData[id].Add((time, prog));
                    }
                }, sw);
                
            scheduler.Add(statCmd);
        }

        server.Start();
        
        int timeout = 5000;
        while (sw.ElapsedMilliseconds < timeout)
        {
            bool allDone = true;
            for (int i = 1; i <= commandCount; i++)
            {
                lock (progressData[i])
                {
                    if (progressData[i].Count < stepsPeriodCommand)
                    {
                        allDone = false;
                        break;
                    }
                }
            }
            if (allDone) break;
            Thread.Sleep(10);
        }

        server.Enqueue(new HardStopCommand(server));
        
        // Вместо Join потока даем серверу гарантированное время на остановку
        Thread.Sleep(200);

        Assert.All(progressData.Values, list => Assert.Equal(stepsPeriodCommand, list.Count));
        GenerateGraphic(progressData, commandCount);
    }

    private void GenerateGraphic(ConcurrentDictionary<int, List<(double TimeMs, double Progress)>> data, int count)
    {
        var plt = new Plot();
        var colors = new[] { "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd" };

        for (int i = 1; i <= count; i++)
        {
            List<(double TimeMs, double Progress)> points;
            lock (data[i])
            {
                points = data[i].OrderBy(x => x.TimeMs).ToList();
            }
            
            double[] xs = points.Select(p => p.TimeMs).ToArray();
            double[] ys = points.Select(p => p.Progress).ToArray();
            
            var sig = plt.Add.SignalXY(xs, ys);
            sig.Color = Color.FromHex(colors[i - 1]);
            sig.LineWidth = 2;
            sig.MarkerSize = 8;
            sig.MarkerShape = MarkerShape.FilledCircle;
            sig.LegendText = $"Команда {i}";
        }

        plt.Title("Выполнение 5 экземпляров TestCommand 3 раза", 16);
        plt.XLabel("Время выполнения (мс)", 14);
        plt.YLabel("Процесс выполнения (%)", 14);
        plt.Axes.AutoScale();
        plt.ShowLegend();
        plt.Grid.MajorLineColor = Color.FromHex("#e0e0e0");

        string projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string outputPath = Path.Combine(projectPath, "GraphicTestCommand.png");
        plt.SavePng(outputPath, 1000, 600);
        
        Assert.True(File.Exists(outputPath));
    }
}

// Выделенный класс для теста 19 задания, чтобы не конфликтовать с CommandsTests.cs
public class Task19TestCommand : ICommand
{
    private readonly int _id;
    private int _counter = 0;

    public Task19TestCommand(int id)
    {
        _id = id;
    }

    public void Execute()
    {
        Thread.Sleep(10);
        Console.WriteLine($"Поток {_id} вызов {++_counter}");
    }
}

public class RepeatedCommand : ILongCommand
{
    private readonly ICommand _inner;
    private readonly int _maxExecutions;
    private int _currentExecutions;

    public bool IsCompleted => _currentExecutions >= _maxExecutions;

    public RepeatedCommand(ICommand inner, int maxExecutions)
    {
        _inner = inner;
        _maxExecutions = maxExecutions;
    }

    public void Execute()
    {
        _inner.Execute();
        _currentExecutions++;
    }
}

public class Statistics : ILongCommand
{
    private readonly ILongCommand _inner;
    private readonly int _id;
    private readonly int _maxSteps;
    private readonly Action<double, double> _progressCallback;
    private readonly Stopwatch _stopwatch;
    private int _currentStep;

    public bool IsCompleted => _inner.IsCompleted;

    public Statistics(ILongCommand inner, int id, int maxSteps, Action<double, double> progressCallback, Stopwatch stopwatch)
    {
        _inner = inner;
        _id = id;
        _maxSteps = maxSteps;
        _progressCallback = progressCallback;
        _stopwatch = stopwatch;
    }

    public void Execute()
    {
        _inner.Execute();
        _currentStep++;
        double progress = (_currentStep / (double)_maxSteps) * 100.0;
        _progressCallback(_stopwatch.ElapsedMilliseconds, progress);
    }
}

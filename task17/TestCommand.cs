namespace task17;

public class TestCommand(int id) : ICommand
{
    private int _counter = 0;

    public void Execute()
    {
        Thread.Sleep(10);
        Console.WriteLine($"Поток {id} вызов {++_counter}");
    }
}

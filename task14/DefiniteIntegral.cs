using System;
using System.Threading;

namespace task14;

public class DefiniteIntegral
{
    private static long _totalSumBits;

    private static void AtomicAddDouble(double value)
    {
        long initialBits, computedBits;
        double initialDouble, computedDouble;

        do
        {
            initialBits = Volatile.Read(ref _totalSumBits);
            initialDouble = BitConverter.Int64BitsToDouble(initialBits);
            computedDouble = initialDouble + value;
            computedBits = BitConverter.DoubleToInt64Bits(computedDouble);
        } 
        while (Interlocked.CompareExchange(ref _totalSumBits, computedBits, initialBits) != initialBits);
    }

    public static double Solve(double a, double b, Func<double, double> function, double step, int threadsNumber)
    {
        Volatile.Write(ref _totalSumBits, BitConverter.DoubleToInt64Bits(0.0));

        int totalSteps = (int)Math.Round((b - a) / step);
        int stepsPerThread = totalSteps / threadsNumber;

        using Barrier barrier = new Barrier(threadsNumber + 1);

        for (int i = 0; i < threadsNumber; i++)
        {
            int threadIndex = i;

            Thread thread = new Thread(() =>
            {
                int startStep = threadIndex * stepsPerThread;
                int endStep = (threadIndex == threadsNumber - 1) ? totalSteps : startStep + stepsPerThread;
                double localSum = 0.0;

                for (int j = startStep; j < endStep; j++)
                {
                    double x1 = a + j * step;
                    double x2 = x1 + step;
                    localSum += (function(x1) + function(x2)) / 2.0 * step;
                }
                AtomicAddDouble(localSum);
                barrier.SignalAndWait();
            });

            thread.Start();
        }

        barrier.SignalAndWait();
        return BitConverter.Int64BitsToDouble(Volatile.Read(ref _totalSumBits));
    }
    
    public static double SolveSingleThread(double a, double b, Func<double, double> function, double step)
    {
        int totalSteps = (int)Math.Round((b - a) / step);
        double sum = 0.0;

        for (int j = 0; j < totalSteps; j++)
        {
            double x1 = a + j * step;
            double x2 = x1 + step;
            sum += (function(x1) + function(x2)) / 2.0 * step;
        }
        return sum;
    }
}

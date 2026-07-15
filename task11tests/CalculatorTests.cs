using System;
using Xunit;
using task11;

namespace task11tests;

public class CalculatorTests
{
    private const string CodeFromTask = @"
        public class Calculator
        {
            public int Add(int a, int b) => a + b;
            public int Minus(int a, int b) => a - b;
            public int Mul(int a, int b) => a * b;
            public int Div(int a, int b) => a / b;
        }";

    [Fact]
    public void TestCalculatorOperations()
    {
        var calculator = GenerationOfClass.CreateCalculator(CodeFromTask);

        Assert.NotNull(calculator);
        Assert.Equal(15, calculator.Add(10, 5));
        Assert.Equal(5, calculator.Minus(10, 5));
        Assert.Equal(50, calculator.Mul(10, 5));
        Assert.Equal(2, calculator.Div(10, 5));
    }

    [Fact]
    public void TestCalculatorWithNegativeNumbers()
    {
        var calculator = GenerationOfClass.CreateCalculator(CodeFromTask);

        Assert.Equal(-12, calculator.Add(-9, -3));
        Assert.Equal(2, calculator.Minus(-7, -9));
        Assert.Equal(-36, calculator.Mul(-12, 3));
        Assert.Equal(1, calculator.Div(-6, -6));
    }

    [Fact]
    public void TestDivideByZeroException()
    {
        var calculator = GenerationOfClass.CreateCalculator(CodeFromTask);
        Assert.Throws<DivideByZeroException>(() => calculator.Div(10, 0));
    }

    [Fact]
    public void TestCompilationErrorOnBadCode()
    {
        string badCode = "public class Calculator { public int Add(int a, int b) => a + b;";
        Assert.Throws<Exception>(() => GenerationOfClass.CreateCalculator(badCode));
    }
}

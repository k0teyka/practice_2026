using System;
using Xunit;
using task14;

namespace task14tests;

public class IntegralTests
{
    [Fact]
    public void TestLinearFunction_SymmetricInterval()
    {
        var X = (double x) => x;
        Assert.Equal(0.0, DefiniteIntegral.Solve(-1.0, 1.0, X, 1e-4, 2), 4); 
    }

    [Fact]
    public void TestLinearFunction_PositiveInterval()
    {
        var X = (double x) => x;
        Assert.Equal(12.5, DefiniteIntegral.Solve(0.0, 5.0, X, 1e-6, 8), 5);
    }

    [Fact]
    public void TestSinFunction_SymmetricInterval()
    {
        var SIN = (double x) => Math.Sin(x);
        Assert.Equal(0.0, DefiniteIntegral.Solve(-1.0, 1.0, SIN, 1e-5, 8), 4);
    }

    [Fact]
    public void TestLogFunction_PositiveInterval()
    {
        var LOG = (double x) => Math.Log(x);
        Assert.Equal(1.0, DefiniteIntegral.Solve(1.0, Math.E, LOG, 1e-5, 4), 4);
    }

    [Fact]
    public void TestConstant_PositiveInterval()
    {
        var CONST = (double x) => 5.0;
        Assert.Equal(10.0, DefiniteIntegral.Solve(0.0, 2.0, CONST, 1e-4, 2), 4);
    }

    [Fact]
    public void TestQuadraticFunction_NegativeInterval()
    {
        var QUADRATIC = (double x) => x * x;
        Assert.Equal(7.0 / 3.0, DefiniteIntegral.Solve(-2.0, -1.0, QUADRATIC, 1e-5, 4), 4);
    }
}

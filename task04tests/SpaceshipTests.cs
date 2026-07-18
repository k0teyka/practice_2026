using Xunit;
using task04;

namespace task04tests;

public class SpaceshipTests
{

    [Fact]
    public void Cruiser_ShouldHaveCorrectStats()
    {
        ISpaceship cruiser = new Cruiser();
        Assert.Equal(50, cruiser.Speed);
        Assert.Equal(100, cruiser.FirePower);
    }

    [Fact]
    public void Fighter_ShouldBeFasterThanCruiser()
    {
        var fighter = new Fighter();
        var cruiser = new Cruiser();
        Assert.True(fighter.Speed > cruiser.Speed);
    }

    [Fact]
    public void Cruiser_ShouldMoveForwardCorrectly()
    {
        var cruiser = new Cruiser();
        
        cruiser.MoveForward();
        cruiser.MoveForward();

        Assert.Equal(100, cruiser.Coordinate);
    }

    [Fact]
    public void Fighter_ShouldRotateAndKeepAngleInBounds()
    {
        var fighter = new Fighter();

        fighter.Rotate(90);
        fighter.Rotate(280);

        Assert.Equal(10, fighter.Angle);
    }

    [Fact]
    public void Cruiser_ShouldRotateWithNegativeAngleCorrectly()
    {
        var cruiser = new Cruiser();

        cruiser.Rotate(-90);

        Assert.Equal(270, cruiser.Angle);
    }

    [Fact]
    public void Ships_ShouldCountShootsCorrectly()
    {
        var fighter = new Fighter();

        fighter.Fire();
        fighter.Fire();
        fighter.Fire();

        Assert.Equal(3, fighter.Shoot);
    }
}

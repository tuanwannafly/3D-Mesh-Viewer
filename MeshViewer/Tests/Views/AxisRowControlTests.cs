using MeshViewer.Views;

namespace MeshViewer.Tests.Views;

public class AxisRowControlTests
{
    [Fact]
    public void ClampToRange_below_minimum_returns_minimum()
    {
        Assert.Equal(-10, AxisRowControl.ClampToRange(-50, -10, 10));
    }

    [Fact]
    public void ClampToRange_above_maximum_returns_maximum()
    {
        Assert.Equal(10, AxisRowControl.ClampToRange(999, -10, 10));
    }

    [Fact]
    public void ClampToRange_within_range_returns_value()
    {
        Assert.Equal(42, AxisRowControl.ClampToRange(42, 0, 100));
    }

    [Fact]
    public void ClampToRange_at_boundary_returns_boundary()
    {
        Assert.Equal(0, AxisRowControl.ClampToRange(0, 0, 100));
        Assert.Equal(100, AxisRowControl.ClampToRange(100, 0, 100));
    }
}
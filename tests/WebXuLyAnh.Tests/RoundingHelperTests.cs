using WebXuLyAnh.Api.Helpers;
using Xunit;

namespace WebXuLyAnh.Tests;

public class RoundingHelperTests
{
    [Theory]
    [InlineData(1, 4, "0.3")]        // 0.25 -> 0.3 (Half-up)
    [InlineData(3, 4, "0.8")]        // 0.75 -> 0.8
    [InlineData(40, 3, "13.3")]      // 13.333... -> 13.3
    [InlineData(70, 3, "23.3")]      // 23.333... -> 23.3
    [InlineData(160, 9, "17.8")]     // 17.777... -> 17.8
    [InlineData(655, 9, "72.8")]     // 72.777... -> 72.8
    [InlineData(450, 9, "50")]       // 50.0 -> 50 (omit .0)
    [InlineData(100, 9, "11.1")]     // 11.111... -> 11.1
    [InlineData(0, 9, "0")]          // 0 -> 0
    [InlineData(5, 2, "2.5")]        // 2.5 -> 2.5
    [InlineData(1, 2, "0.5")]        // 0.5 -> 0.5
    [InlineData(-1, 4, "-0.3")]      // -0.25 -> -0.3 (away from zero)
    [InlineData(-1, 10, "-0.1")]     // -0.1
    [InlineData(-1, 100, "0")]       // -0.01 -> 0 (no -0)
    public void FormatRounded1_ReturnsExpectedString(int numerator, int denominator, string expected)
    {
        string actual = RoundingHelper.FormatRounded1(numerator, denominator);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 4, 0)]            // 0.25 -> 0
    [InlineData(3, 4, 1)]            // 0.75 -> 1
    [InlineData(40, 3, 13)]          // 13.333 -> 13
    [InlineData(160, 9, 18)]         // 17.777 -> 18
    [InlineData(5, 2, 3)]            // 2.5 -> 3
    [InlineData(1, 2, 1)]            // 0.5 -> 1
    [InlineData(0, 1, 0)]            // 0 -> 0
    [InlineData(-5, 2, -3)]          // -2.5 -> -3
    public void FormatRoundedInt_ReturnsExpectedInt(int numerator, int denominator, int expected)
    {
        int actual = RoundingHelper.FormatRoundedInt(numerator, denominator);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(12, 18, 6)]
    [InlineData(9, 27, 9)]
    [InlineData(7, 13, 1)]
    [InlineData(0, 5, 5)]
    public void MathHelper_Gcd_ReturnsExpected(int a, int b, int expected)
    {
        int actual = MathHelper.Gcd(a, b);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(120, 9, 40, 3, "40/3")]
    [InlineData(450, 9, 50, 1, "50")]
    [InlineData(0, 9, 0, 1, "0")]
    [InlineData(10, 4, 5, 2, "5/2")]
    public void MathHelper_SimplifyFraction_ReturnsExpected(
        int num, int den,
        int expectedNum, int expectedDen, string expectedDisplay)
    {
        var (sNum, sDen, sDisplay) = MathHelper.SimplifyFraction(num, den);
        Assert.Equal(expectedNum, sNum);
        Assert.Equal(expectedDen, sDen);
        Assert.Equal(expectedDisplay, sDisplay);
    }
}

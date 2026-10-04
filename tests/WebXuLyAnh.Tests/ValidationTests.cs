using System.Text.Json;
using WebXuLyAnh.Api.Validators;
using Xunit;

namespace WebXuLyAnh.Tests;

public class ValidationTests
{
    private static JsonElement ParseJson(string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }

    [Fact]
    public void VAL_01_EmptyMatrix_Returns_EMPTY_MATRIX()
    {
        var json = ParseJson("""
        {
            "matrix": [],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "EMPTY_MATRIX");
    }

    [Fact]
    public void VAL_02_RaggedMatrix_Returns_RAGGED_MATRIX()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [1, 2, 3],
                [4, 5]
            ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "RAGGED_MATRIX");
    }

    [Fact]
    public void VAL_03_PixelOutOfRange_Upper_Returns_PIXEL_OUT_OF_RANGE()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 300],
                [20, 30]
            ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        var err = errors.FirstOrDefault(e => e.Code == "PIXEL_OUT_OF_RANGE");
        Assert.NotNull(err);
        Assert.Equal(0, err.Row);
        Assert.Equal(1, err.Col);
    }

    [Fact]
    public void VAL_04_PixelOutOfRange_Negative_Returns_PIXEL_OUT_OF_RANGE()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 20],
                [-5, 30]
            ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        var err = errors.FirstOrDefault(e => e.Code == "PIXEL_OUT_OF_RANGE");
        Assert.NotNull(err);
        Assert.Equal(1, err.Row);
        Assert.Equal(0, err.Col);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("\"abc\"")]
    [InlineData("null")]
    public void VAL_05_NonIntegerOrDecimal_Returns_EMPTY_OR_NON_INTEGER(string invalidCell)
    {
        var json = ParseJson($$"""
        {
            "matrix": [
                [10, {{invalidCell}}],
                [20, 30]
            ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        var err = errors.FirstOrDefault(e => e.Code == "EMPTY_OR_NON_INTEGER");
        Assert.NotNull(err);
        Assert.Equal(0, err.Row);
        Assert.Equal(1, err.Col);
    }

    [Fact]
    public void VAL_06_DimensionExceeds20_Returns_INVALID_MATRIX_DIMENSIONS()
    {
        // 21 rows
        var rows = string.Join(",", Enumerable.Repeat("[10, 20]", 21));
        var json = ParseJson($$"""
        {
            "matrix": [ {{rows}} ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "INVALID_MATRIX_DIMENSIONS");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-1)]
    public void VAL_07_InvalidKValue_Returns_INVALID_K_VALUE(int k)
    {
        var json = ParseJson($$"""
        {
            "matrix": [
                [10, 20],
                [30, 40]
            ],
            "k": {{k}},
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "INVALID_K_VALUE");
    }

    [Fact]
    public void VAL_08_InvalidMethod_Returns_INVALID_METHOD()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 20],
                [30, 40]
            ],
            "k": 3,
            "method": "SOBEL"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "INVALID_METHOD");
    }

    [Fact]
    public void VAL_09_MissingPrewittKernel_WhenKNot3_Returns_MISSING_PREWITT_KERNEL()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 20],
                [30, 40]
            ],
            "k": 5,
            "method": "PREWITT"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "MISSING_PREWITT_KERNEL");
    }

    [Fact]
    public void VAL_10_InvalidKernelDimensions_Returns_INVALID_KERNEL_DIMENSIONS()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 20],
                [30, 40]
            ],
            "k": 3,
            "method": "PREWITT",
            "prewittConfig": {
                "kernelX": [
                    [-1, 1],
                    [-1, 1]
                ],
                "kernelY": [
                    [-1, -1],
                    [1, 1]
                ]
            }
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out _, out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Code == "INVALID_KERNEL_DIMENSIONS");
    }

    [Fact]
    public void Warning_When_KGreaterThanMatrixDimension()
    {
        var json = ParseJson("""
        {
            "matrix": [
                [10, 20],
                [30, 40]
            ],
            "k": 3,
            "method": "MEAN"
        }
        """);

        bool success = MatrixValidator.ValidateFilterJson(json, out var request, out var warnings, out var errors);

        Assert.True(success);
        Assert.Empty(errors);
        Assert.Single(warnings);
        Assert.Equal("K_GREATER_THAN_MATRIX_DIMENSION", warnings[0].Code);
    }
}

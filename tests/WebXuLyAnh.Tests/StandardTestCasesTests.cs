using WebXuLyAnh.Api.Models;
using WebXuLyAnh.Api.Services;
using Xunit;

namespace WebXuLyAnh.Tests;

public class StandardTestCasesTests
{
    private readonly IFilterService _filterService = new FilterService();

    [Fact]
    public void TestCase1_Mean_3x3_MatchesBenchmark()
    {
        int[][] matrix = [
            [10, 20, 30],
            [40, 50, 60],
            [70, 80, 90]
        ];

        var request = new MatrixFilterRequest(matrix, 3, "MEAN");
        var response = _filterService.ApplyFilter(request);

        Assert.True(response.Success);
        Assert.Equal(3, response.MatrixSize.Rows);
        Assert.Equal(3, response.MatrixSize.Cols);
        Assert.Equal(1, response.Padding.PadTop);
        Assert.Equal(1, response.Padding.PadBottom);

        // Expected Rounded1
        string[][] expectedRounded1 = [
            ["13.3", "23.3", "17.8"],
            ["30",   "50",   "36.7"],
            ["26.7", "43.3", "31.1"]
        ];

        // Expected RoundedInt
        int[][] expectedRoundedInt = [
            [13, 23, 18],
            [30, 50, 37],
            [27, 43, 31]
        ];

        // Expected Exact Fractions
        string[][] expectedExact = [
            ["40/3", "70/3", "160/9"],
            ["30",   "50",   "110/3"],
            ["80/3", "130/3", "280/9"]
        ];

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = response.Results[r][c];
                Assert.Equal(expectedRounded1[r][c], cell.Rounded1);
                Assert.Equal(expectedRoundedInt[r][c], cell.RoundedInt);
                Assert.Equal(expectedExact[r][c], cell.Exact.Display);
                Assert.NotNull(cell.Step?.Mean);
            }
        }

        // Check specific step details at (0, 0)
        var cell00 = response.Results[0][0];
        Assert.Equal(120, cell00.Step!.Mean!.Sum);
        Assert.Equal(9, cell00.Step.Mean.Count);
        Assert.Equal("120/9", cell00.Step.Mean.Fraction);
        Assert.Equal("40/3", cell00.Step.Mean.ReducedFraction);
    }

    [Fact]
    public void TestCase1_Median_3x3_MatchesBenchmark()
    {
        int[][] matrix = [
            [10, 20, 30],
            [40, 50, 60],
            [70, 80, 90]
        ];

        var request = new MatrixFilterRequest(matrix, 3, "MEDIAN");
        var response = _filterService.ApplyFilter(request);

        int[][] expectedMedian = [
            [ 0, 20,  0],
            [20, 50, 30],
            [ 0, 50,  0]
        ];

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = response.Results[r][c];
                Assert.Equal(expectedMedian[r][c], cell.RoundedInt);
                Assert.Equal(expectedMedian[r][c].ToString(), cell.Rounded1);
                Assert.NotNull(cell.Step?.Median);
            }
        }

        // Center cell (1, 1) median step
        var center = response.Results[1][1];
        Assert.Equal(50, center.RoundedInt);
        Assert.Equal([4], center.Step!.Median!.PickedIndices);
        Assert.Equal([50], center.Step.Median.PickedValues);
    }

    [Fact]
    public void TestCase1_Prewitt_3x3_MatchesBenchmark()
    {
        int[][] matrix = [
            [10, 20, 30],
            [40, 50, 60],
            [70, 80, 90]
        ];

        var request = new MatrixFilterRequest(matrix, 3, "PREWITT");
        var response = _filterService.ApplyFilter(request);

        int[][] expectedGx = [
            [-70,  -40,   70],
            [-150, -60,  150],
            [-130, -40,  130]
        ];

        int[][] expectedGy = [
            [-90, -150, -110],
            [-120, -180, -120],
            [ 90,  150,  110]
        ];

        int[][] expectedG = [
            [160, 190, 180],
            [270, 240, 270],
            [220, 190, 240]
        ];

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = response.Results[r][c];
                Assert.Equal(expectedG[r][c], cell.RoundedInt);
                Assert.NotNull(cell.Step?.Prewitt);
                Assert.Equal(expectedGx[r][c], cell.Step.Prewitt.Gx);
                Assert.Equal(expectedGy[r][c], cell.Step.Prewitt.Gy);
                Assert.Equal(expectedG[r][c], cell.Step.Prewitt.G);
            }
        }
    }

    [Fact]
    public void TestCase2_SaltAndPepperNoise_3x3_MatchesBenchmark()
    {
        int[][] matrix = [
            [50,  50, 50],
            [50, 255, 50],
            [50,  50, 50]
        ];

        // 1. Mean
        var meanRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 3, "MEAN"));
        Assert.Equal("72.8", meanRes.Results[1][1].Rounded1);

        // 2. Median (noise completely filtered out)
        var medianRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 3, "MEDIAN"));
        Assert.Equal(50, medianRes.Results[1][1].RoundedInt);

        // 3. Prewitt (center is 0 due to symmetry)
        var prewittRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 3, "PREWITT"));
        Assert.Equal(0, prewittRes.Results[1][1].RoundedInt);
    }

    [Fact]
    public void TestCase3_EvenK2x2_MatchesBenchmark()
    {
        int[][] matrix = [
            [1, 2],
            [3, 4]
        ];

        // Mean
        var meanRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 2, "MEAN"));
        Assert.Equal("0.3", meanRes.Results[0][0].Rounded1);
        Assert.Equal("0.8", meanRes.Results[0][1].Rounded1);
        Assert.Equal("1",   meanRes.Results[1][0].Rounded1);
        Assert.Equal("2.5", meanRes.Results[1][1].Rounded1);

        Assert.Equal(0, meanRes.Results[0][0].RoundedInt);
        Assert.Equal(1, meanRes.Results[0][1].RoundedInt);
        Assert.Equal(1, meanRes.Results[1][0].RoundedInt);
        Assert.Equal(3, meanRes.Results[1][1].RoundedInt);

        // Median
        var medianRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 2, "MEDIAN"));
        Assert.Equal("0",   medianRes.Results[0][0].Rounded1);
        Assert.Equal("0.5", medianRes.Results[0][1].Rounded1);
        Assert.Equal("0.5", medianRes.Results[1][0].Rounded1);
        Assert.Equal("2.5", medianRes.Results[1][1].Rounded1);

        // Prewitt with custom 2x2 kernel
        int[][] kx = [
            [-1, 1],
            [-1, 1]
        ];
        int[][] ky = [
            [-1, -1],
            [ 1,  1]
        ];

        var prewittRes = _filterService.ApplyFilter(new MatrixFilterRequest(
            Matrix: matrix,
            K: 2,
            Method: "PREWITT",
            PrewittConfig: new PrewittConfigDto(kx, ky)
        ));

        int[][] expectedG = [
            [2, 4],
            [6, 6]
        ];

        for (int r = 0; r < 2; r++)
        {
            for (int c = 0; c < 2; c++)
            {
                Assert.Equal(expectedG[r][c], prewittRes.Results[r][c].RoundedInt);
            }
        }
    }

    [Fact]
    public void TestCase4_PrewittVerticalEdge_4x4_MatchesBenchmark()
    {
        int[][] matrix = [
            [10, 10, 100, 100],
            [10, 10, 100, 100],
            [10, 10, 100, 100],
            [10, 10, 100, 100]
        ];

        var res = _filterService.ApplyFilter(new MatrixFilterRequest(matrix, 3, "PREWITT"));

        int[][] expectedG = [
            [ 40, 300, 390, 400],
            [ 30, 270, 270, 300],
            [ 30, 270, 270, 300],
            [ 40, 300, 390, 400]
        ];

        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                Assert.Equal(expectedG[r][c], res.Results[r][c].RoundedInt);
            }
        }
    }

    [Fact]
    public void TestCase5_Matrix1x1_And_KGreaterThanDimension_MatchesBenchmark()
    {
        // 1x1 k=1
        int[][] matrix1x1 = [[100]];
        var mean1 = _filterService.ApplyFilter(new MatrixFilterRequest(matrix1x1, 1, "MEAN"));
        Assert.Equal("100", mean1.Results[0][0].Rounded1);

        var med1 = _filterService.ApplyFilter(new MatrixFilterRequest(matrix1x1, 1, "MEDIAN"));
        Assert.Equal("100", med1.Results[0][0].Rounded1);

        // 2x2 with k=3
        int[][] matrix2x2 = [
            [10, 20],
            [30, 40]
        ];
        var warnRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix2x2, 3, "MEAN"));
        Assert.Single(warnRes.Warnings);
        Assert.Equal("K_GREATER_THAN_MATRIX_DIMENSION", warnRes.Warnings[0].Code);

        // All 4 cells should be 11.1
        for (int r = 0; r < 2; r++)
        {
            for (int c = 0; c < 2; c++)
            {
                Assert.Equal("11.1", warnRes.Results[r][c].Rounded1);
                Assert.Equal("100/9", warnRes.Results[r][c].Exact.Display);
            }
        }

        // Median 2x2 with k=3: all 4 cells are 0
        var warnMedRes = _filterService.ApplyFilter(new MatrixFilterRequest(matrix2x2, 3, "MEDIAN"));
        for (int r = 0; r < 2; r++)
        {
            for (int c = 0; c < 2; c++)
            {
                Assert.Equal(0, warnMedRes.Results[r][c].RoundedInt);
            }
        }

        // Prewitt 2x2 with k=3:
        // [[130, 110], [90, 70]]
        var warnPrewitt = _filterService.ApplyFilter(new MatrixFilterRequest(matrix2x2, 3, "PREWITT"));
        Assert.Equal(130, warnPrewitt.Results[0][0].RoundedInt);
        Assert.Equal(110, warnPrewitt.Results[0][1].RoundedInt);
        Assert.Equal(90,  warnPrewitt.Results[1][0].RoundedInt);
        Assert.Equal(70,  warnPrewitt.Results[1][1].RoundedInt);
    }

    [Fact]
    public void BatchFilter_AppliesAllRequestedMethods()
    {
        int[][] matrix = [
            [10, 20, 30],
            [40, 50, 60],
            [70, 80, 90]
        ];

        var request = new BatchFilterRequest(
            Matrix: matrix,
            K: 3,
            Methods: ["MEAN", "MEDIAN", "PREWITT"]
        );

        var response = _filterService.ApplyBatchFilter(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data["mean"]);
        Assert.NotNull(response.Data["median"]);
        Assert.NotNull(response.Data["prewitt"]);

        Assert.Equal("13.3", response.Data["mean"]!.Results[0][0].Rounded1);
        Assert.Equal(0, response.Data["median"]!.Results[0][0].RoundedInt);
        Assert.Equal(160, response.Data["prewitt"]!.Results[0][0].RoundedInt);
    }
}

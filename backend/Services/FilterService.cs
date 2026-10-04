using WebXuLyAnh.Api.Helpers;
using WebXuLyAnh.Api.Models;

namespace WebXuLyAnh.Api.Services;

public class FilterService : IFilterService
{
    private static readonly int[][] DefaultKernelX3x3 = [
        [-1, 0, 1],
        [-1, 0, 1],
        [-1, 0, 1]
    ];

    private static readonly int[][] DefaultKernelY3x3 = [
        [-1, -1, -1],
        [ 0,  0,  0],
        [ 1,  1,  1]
    ];

    public FilterResponse ApplyFilter(MatrixFilterRequest request, List<WarningDto>? warnings = null)
    {
        int rows = request.Matrix.Length;
        int cols = request.Matrix[0].Length;
        int k = request.K;
        string method = request.Method.Trim().ToUpperInvariant();
        bool includeSteps = request.Options?.IncludeSteps ?? true;

        var matrixSize = new MatrixSizeDto(rows, cols);

        int padTop = k / 2;
        int padLeft = k / 2;
        int padBottom = k - 1 - (k / 2);
        int padRight = k - 1 - (k / 2);
        var padding = new PaddingInfoDto(padTop, padBottom, padLeft, padRight);

        var finalWarnings = new List<WarningDto>(warnings ?? []);
        if (k > rows || k > cols)
        {
            string warnCode = "K_GREATER_THAN_MATRIX_DIMENSION";
            if (!finalWarnings.Any(w => w.Code == warnCode))
            {
                finalWarnings.Add(new WarningDto(
                    warnCode,
                    $"Kích thước cửa sổ lọc (k={k}) lớn hơn kích thước ma trận ({rows}×{cols}). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
                ));
            }
        }

        // Setup Prewitt kernels if needed
        int[][]? kernelXOriginal = null;
        int[][]? kernelYOriginal = null;
        int[][]? kernelXFlipped = null;
        int[][]? kernelYFlipped = null;

        if (method == "PREWITT")
        {
            if (k == 3)
            {
                kernelXOriginal = request.PrewittConfig?.KernelX ?? DefaultKernelX3x3;
                kernelYOriginal = request.PrewittConfig?.KernelY ?? DefaultKernelY3x3;
            }
            else
            {
                kernelXOriginal = request.PrewittConfig?.KernelX!;
                kernelYOriginal = request.PrewittConfig?.KernelY!;
            }

            kernelXFlipped = FlipKernel180(kernelXOriginal, k);
            kernelYFlipped = FlipKernel180(kernelYOriginal, k);
        }

        var results = new CellResultDto[rows][];

        for (int i = 0; i < rows; i++)
        {
            results[i] = new CellResultDto[cols];
            for (int j = 0; j < cols; j++)
            {
                int[][] window = ExtractWindow(request.Matrix, rows, cols, i, j, k, padTop, padLeft, padBottom, padRight);
                string displayCoords = $"Hàng {i + 1}, Cột {j + 1}";

                switch (method)
                {
                    case "MEAN":
                        results[i][j] = ComputeMean(i, j, displayCoords, window, k, includeSteps);
                        break;

                    case "MEDIAN":
                        results[i][j] = ComputeMedian(i, j, displayCoords, window, k, includeSteps);
                        break;

                    case "PREWITT":
                        results[i][j] = ComputePrewitt(
                            i, j, displayCoords, window, k,
                            kernelXOriginal!, kernelYOriginal!,
                            kernelXFlipped!, kernelYFlipped!,
                            includeSteps);
                        break;

                    default:
                        throw new ArgumentException($"Phương pháp lọc '{method}' không được hỗ trợ.");
                }
            }
        }

        return new FilterResponse(
            Success: true,
            Method: method,
            K: k,
            MatrixSize: matrixSize,
            Padding: padding,
            Warnings: finalWarnings,
            Results: results
        );
    }

    public BatchFilterResponse ApplyBatchFilter(BatchFilterRequest request, List<WarningDto>? warnings = null)
    {
        int rows = request.Matrix.Length;
        int cols = request.Matrix[0].Length;
        int k = request.K;
        var matrixSize = new MatrixSizeDto(rows, cols);

        var finalWarnings = new List<WarningDto>(warnings ?? []);
        if (k > rows || k > cols)
        {
            string warnCode = "K_GREATER_THAN_MATRIX_DIMENSION";
            if (!finalWarnings.Any(w => w.Code == warnCode))
            {
                finalWarnings.Add(new WarningDto(
                    warnCode,
                    $"Kích thước cửa sổ lọc (k={k}) lớn hơn kích thước ma trận ({rows}×{cols}). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
                ));
            }
        }

        var data = new Dictionary<string, FilterResponse?>();

        foreach (var method in request.Methods)
        {
            string upper = method.Trim().ToUpperInvariant();
            string key = upper.ToLowerInvariant();

            var singleRequest = new MatrixFilterRequest(
                Matrix: request.Matrix,
                K: k,
                Method: upper,
                PrewittConfig: request.PrewittConfig,
                Options: request.Options
            );

            data[key] = ApplyFilter(singleRequest, finalWarnings);
        }

        return new BatchFilterResponse(
            Success: true,
            MatrixSize: matrixSize,
            K: k,
            Warnings: finalWarnings,
            Data: data
        );
    }

    private static int[][] ExtractWindow(
        int[][] matrix, int rows, int cols,
        int cellRow, int cellCol,
        int k, int padTop, int padLeft, int padBottom, int padRight)
    {
        int[][] window = new int[k][];
        for (int u = 0; u < k; u++)
        {
            window[u] = new int[k];
            int r = cellRow - padTop + u;
            for (int v = 0; v < k; v++)
            {
                int c = cellCol - padLeft + v;
                if (r >= 0 && r < rows && c >= 0 && c < cols)
                {
                    window[u][v] = matrix[r][c];
                }
                else
                {
                    window[u][v] = 0;
                }
            }
        }
        return window;
    }

    private static int[][] FlipKernel180(int[][] kernel, int k)
    {
        int[][] flipped = new int[k][];
        for (int u = 0; u < k; u++)
        {
            flipped[u] = new int[k];
            for (int v = 0; v < k; v++)
            {
                flipped[u][v] = kernel[k - 1 - u][k - 1 - v];
            }
        }
        return flipped;
    }

    private static CellResultDto ComputeMean(
        int row, int col, string displayCoords,
        int[][] window, int k, bool includeSteps)
    {
        int count = k * k;
        int sum = 0;
        for (int u = 0; u < k; u++)
        {
            for (int v = 0; v < k; v++)
            {
                sum += window[u][v];
            }
        }

        var (simNum, simDen, display) = MathHelper.SimplifyFraction(sum, count);
        var exact = new FractionDto(simNum, simDen, display);
        string rounded1 = RoundingHelper.FormatRounded1(sum, count);
        int roundedInt = RoundingHelper.FormatRoundedInt(sum, count);

        StepDetailDto? step = null;
        if (includeSteps)
        {
            string elementsStr = string.Join(" + ", window.SelectMany(r => r));
            string formula;
            if (simDen == 1)
            {
                formula = $"({elementsStr}) / {count} = {sum} / {count} = {display}";
            }
            else if (display != $"{sum}/{count}")
            {
                formula = $"({elementsStr}) / {count} = {sum} / {count} = {display} ≈ {rounded1}";
            }
            else
            {
                formula = $"({elementsStr}) / {count} = {sum} / {count} ≈ {rounded1}";
            }

            var meanStep = new MeanStepDto(
                Sum: sum,
                Count: count,
                Fraction: $"{sum}/{count}",
                ReducedFraction: display,
                Formula: formula
            );

            step = new StepDetailDto(
                Window: window,
                Mean: meanStep
            );
        }

        return new CellResultDto(
            Row: row,
            Col: col,
            DisplayCoordinates: displayCoords,
            Exact: exact,
            Rounded1: rounded1,
            RoundedInt: roundedInt,
            Step: step
        );
    }

    private static CellResultDto ComputeMedian(
        int row, int col, string displayCoords,
        int[][] window, int k, bool includeSteps)
    {
        int[] rawElements = window.SelectMany(r => r).ToArray();
        int[] sortedElements = (int[])rawElements.Clone();
        Array.Sort(sortedElements);

        int count = rawElements.Length;
        bool isEven = (count % 2 == 0);

        FractionDto exact;
        string rounded1;
        int roundedInt;
        int[] pickedIndices;
        int[] pickedValues;
        string formula;

        if (!isEven)
        {
            int medianIndex = count / 2;
            int medianValue = sortedElements[medianIndex];
            pickedIndices = [medianIndex];
            pickedValues = [medianValue];
            exact = new FractionDto(medianValue, 1, medianValue.ToString());
            rounded1 = medianValue.ToString();
            roundedInt = medianValue;
            formula = $"Dãy sắp xếp: [{string.Join(", ", sortedElements)}]. Số phần tử lẻ ({count}), lấy phần tử thứ {medianIndex + 1} tại chỉ số {medianIndex}: giá trị = {medianValue}.";
        }
        else
        {
            int idx1 = count / 2 - 1;
            int idx2 = count / 2;
            int val1 = sortedElements[idx1];
            int val2 = sortedElements[idx2];
            pickedIndices = [idx1, idx2];
            pickedValues = [val1, val2];
            int sum = val1 + val2;

            var (simNum, simDen, display) = MathHelper.SimplifyFraction(sum, 2);
            exact = new FractionDto(simNum, simDen, display);
            rounded1 = RoundingHelper.FormatRounded1(sum, 2);
            roundedInt = RoundingHelper.FormatRoundedInt(sum, 2);
            formula = $"Dãy sắp xếp: [{string.Join(", ", sortedElements)}]. Số phần tử chẵn ({count}), lấy trung bình 2 phần tử giữa ({val1} + {val2}) / 2 = {display}.";
        }

        StepDetailDto? step = null;
        if (includeSteps)
        {
            var medianStep = new MedianStepDto(
                RawElements: rawElements,
                SortedElements: sortedElements,
                PickedIndices: pickedIndices,
                PickedValues: pickedValues,
                IsEven: isEven,
                Formula: formula
            );

            step = new StepDetailDto(
                Window: window,
                Median: medianStep
            );
        }

        return new CellResultDto(
            Row: row,
            Col: col,
            DisplayCoordinates: displayCoords,
            Exact: exact,
            Rounded1: rounded1,
            RoundedInt: roundedInt,
            Step: step
        );
    }

    private static CellResultDto ComputePrewitt(
        int row, int col, string displayCoords,
        int[][] window, int k,
        int[][] kernelXOriginal, int[][] kernelYOriginal,
        int[][] kernelXFlipped, int[][] kernelYFlipped,
        bool includeSteps)
    {
        int gx = 0;
        int gy = 0;
        var termsGx = new List<string>();
        var termsGy = new List<string>();

        for (int u = 0; u < k; u++)
        {
            for (int v = 0; v < k; v++)
            {
                int w = window[u][v];
                int kx = kernelXFlipped[u][v];
                int ky = kernelYFlipped[u][v];

                gx += w * kx;
                gy += w * ky;

                if (w != 0 && kx != 0)
                {
                    termsGx.Add(kx < 0 ? $"{w}*({kx})" : $"{w}*{kx}");
                }
                if (w != 0 && ky != 0)
                {
                    termsGy.Add(ky < 0 ? $"{w}*({ky})" : $"{w}*{ky}");
                }
            }
        }

        int absGx = Math.Abs(gx);
        int absGy = Math.Abs(gy);
        int g = absGx + absGy;

        var exact = new FractionDto(g, 1, g.ToString());
        string rounded1 = g.ToString();
        int roundedInt = g;

        StepDetailDto? step = null;
        if (includeSteps)
        {
            string exprGx = termsGx.Count > 0 ? string.Join(" + ", termsGx) : "0";
            string exprGy = termsGy.Count > 0 ? string.Join(" + ", termsGy) : "0";

            string formulaGx = $"Tích chập Gx (kernel lật 180°): {exprGx} = {gx}";
            string formulaGy = $"Tích chập Gy (kernel lật 180°): {exprGy} = {gy}";
            string formulaG = $"|Gx| + |Gy| = |{gx}| + |{gy}| = {absGx} + {absGy} = {g}";

            var prewittStep = new PrewittStepDto(
                KernelXOriginal: kernelXOriginal,
                KernelYOriginal: kernelYOriginal,
                KernelXFlipped: kernelXFlipped,
                KernelYFlipped: kernelYFlipped,
                Gx: gx,
                Gy: gy,
                AbsGx: absGx,
                AbsGy: absGy,
                G: g,
                FormulaGx: formulaGx,
                FormulaGy: formulaGy,
                FormulaG: formulaG
            );

            step = new StepDetailDto(
                Window: window,
                Prewitt: prewittStep
            );
        }

        return new CellResultDto(
            Row: row,
            Col: col,
            DisplayCoordinates: displayCoords,
            Exact: exact,
            Rounded1: rounded1,
            RoundedInt: roundedInt,
            Step: step
        );
    }
}

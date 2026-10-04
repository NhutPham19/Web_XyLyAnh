using System.Text.Json;
using WebXuLyAnh.Api.Models;

namespace WebXuLyAnh.Api.Validators;

public static class MatrixValidator
{
    public static bool ValidateFilterJson(
        JsonElement root,
        out MatrixFilterRequest? request,
        out List<WarningDto> warnings,
        out List<ValidationErrorDetail> errors)
    {
        warnings = new List<WarningDto>();
        errors = new List<ValidationErrorDetail>();
        request = null;

        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new ValidationErrorDetail("request", null, null, "INVALID_REQUEST_BODY", "Request body phải là JSON object."));
            return false;
        }

        // 1. Validate Matrix
        bool matrixOk = ValidateMatrixElement(root, errors, out int[][]? matrix, out int rows, out int cols);

        // 2. Validate K
        bool kOk = ValidateKElement(root, errors, out int k);

        // 3. Validate Method
        bool methodOk = ValidateMethodElement(root, errors, out string method);

        // 4. Validate Prewitt Config
        PrewittConfigDto? prewittConfig = null;
        if (methodOk && method == "PREWITT")
        {
            ValidatePrewittConfigElement(root, kOk ? k : 3, errors, out prewittConfig);
        }

        // 5. Options
        bool includeSteps = true;
        if (root.TryGetProperty("options", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Object)
        {
            if (optionsProp.TryGetProperty("includeSteps", out var incProp) &&
                (incProp.ValueKind == JsonValueKind.True || incProp.ValueKind == JsonValueKind.False))
            {
                includeSteps = incProp.GetBoolean();
            }
        }

        // 6. Warnings
        if (matrixOk && kOk && (k > rows || k > cols))
        {
            warnings.Add(new WarningDto(
                "K_GREATER_THAN_MATRIX_DIMENSION",
                $"Kích thước cửa sổ lọc (k={k}) lớn hơn kích thước ma trận ({rows}×{cols}). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
            ));
        }

        if (errors.Count > 0)
        {
            return false;
        }

        request = new MatrixFilterRequest(
            Matrix: matrix!,
            K: k,
            Method: method,
            PrewittConfig: prewittConfig,
            Options: new FilterOptionsDto(includeSteps)
        );
        return true;
    }

    public static bool ValidateBatchFilterJson(
        JsonElement root,
        out BatchFilterRequest? request,
        out List<WarningDto> warnings,
        out List<ValidationErrorDetail> errors)
    {
        warnings = new List<WarningDto>();
        errors = new List<ValidationErrorDetail>();
        request = null;

        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new ValidationErrorDetail("request", null, null, "INVALID_REQUEST_BODY", "Request body phải là JSON object."));
            return false;
        }

        bool matrixOk = ValidateMatrixElement(root, errors, out int[][]? matrix, out int rows, out int cols);
        bool kOk = ValidateKElement(root, errors, out int k);

        // Validate Methods array
        List<string> methods = new();
        if (!root.TryGetProperty("methods", out var methodsProp) ||
            methodsProp.ValueKind != JsonValueKind.Array ||
            methodsProp.GetArrayLength() == 0)
        {
            errors.Add(new ValidationErrorDetail(
                "methods", null, null, "INVALID_METHOD",
                "Danh sách phương pháp không hợp lệ hoặc để trống. Chỉ chấp nhận MEAN, MEDIAN hoặc PREWITT."
            ));
        }
        else
        {
            foreach (var elem in methodsProp.EnumerateArray())
            {
                if (elem.ValueKind != JsonValueKind.String)
                {
                    errors.Add(new ValidationErrorDetail(
                        "methods", null, null, "INVALID_METHOD",
                        "Phương pháp lọc phải là chuỗi ký tự (MEAN, MEDIAN hoặc PREWITT)."
                    ));
                    break;
                }
                var m = elem.GetString()?.Trim().ToUpperInvariant() ?? "";
                if (m != "MEAN" && m != "MEDIAN" && m != "PREWITT")
                {
                    errors.Add(new ValidationErrorDetail(
                        "methods", null, null, "INVALID_METHOD",
                        $"Phương pháp lọc '{elem.GetString()}' không hợp lệ. Chỉ chấp nhận MEAN, MEDIAN hoặc PREWITT."
                    ));
                    break;
                }
                if (!methods.Contains(m))
                {
                    methods.Add(m);
                }
            }
        }

        PrewittConfigDto? prewittConfig = null;
        if (methods.Contains("PREWITT"))
        {
            ValidatePrewittConfigElement(root, kOk ? k : 3, errors, out prewittConfig);
        }

        bool includeSteps = true;
        if (root.TryGetProperty("options", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Object)
        {
            if (optionsProp.TryGetProperty("includeSteps", out var incProp) &&
                (incProp.ValueKind == JsonValueKind.True || incProp.ValueKind == JsonValueKind.False))
            {
                includeSteps = incProp.GetBoolean();
            }
        }

        if (matrixOk && kOk && (k > rows || k > cols))
        {
            warnings.Add(new WarningDto(
                "K_GREATER_THAN_MATRIX_DIMENSION",
                $"Kích thước cửa sổ lọc (k={k}) lớn hơn kích thước ma trận ({rows}×{cols}). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
            ));
        }

        if (errors.Count > 0)
        {
            return false;
        }

        request = new BatchFilterRequest(
            Matrix: matrix!,
            K: k,
            Methods: methods,
            PrewittConfig: prewittConfig,
            Options: new FilterOptionsDto(includeSteps)
        );
        return true;
    }

    private static bool ValidateMatrixElement(
        JsonElement root,
        List<ValidationErrorDetail> errors,
        out int[][]? matrix,
        out int rows,
        out int cols)
    {
        matrix = null;
        rows = 0;
        cols = 0;

        if (!root.TryGetProperty("matrix", out var matrixProp) ||
            matrixProp.ValueKind != JsonValueKind.Array ||
            matrixProp.GetArrayLength() == 0)
        {
            errors.Add(new ValidationErrorDetail("matrix", null, null, "EMPTY_MATRIX", "Ma trận không được để trống."));
            return false;
        }

        rows = matrixProp.GetArrayLength();
        if (rows < 1 || rows > 20)
        {
            errors.Add(new ValidationErrorDetail("matrix", null, null, "INVALID_MATRIX_DIMENSIONS", "Kích thước ma trận phải từ 1×1 đến tối đa 20×20."));
            return false;
        }

        var firstRow = matrixProp[0];
        if (firstRow.ValueKind != JsonValueKind.Array || firstRow.GetArrayLength() == 0)
        {
            errors.Add(new ValidationErrorDetail("matrix", 0, null, "INVALID_MATRIX_DIMENSIONS", "Hàng 1 của ma trận không hợp lệ hoặc để trống."));
            return false;
        }

        cols = firstRow.GetArrayLength();
        if (cols < 1 || cols > 20)
        {
            errors.Add(new ValidationErrorDetail("matrix", null, null, "INVALID_MATRIX_DIMENSIONS", "Kích thước ma trận phải từ 1×1 đến tối đa 20×20."));
            return false;
        }

        bool hasRagged = false;
        bool hasCellError = false;
        int[][] tempMatrix = new int[rows][];

        for (int r = 0; r < rows; r++)
        {
            var rowElem = matrixProp[r];
            if (rowElem.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationErrorDetail("matrix", r, null, "INVALID_MATRIX_DIMENSIONS", $"Hàng {r + 1} không phải là mảng."));
                return false;
            }

            if (rowElem.GetArrayLength() != cols)
            {
                hasRagged = true;
                break;
            }

            tempMatrix[r] = new int[cols];
            for (int c = 0; c < cols; c++)
            {
                var cell = rowElem[c];
                string raw = cell.GetRawText();
                if (cell.ValueKind != JsonValueKind.Number ||
                    !cell.TryGetInt32(out int val) ||
                    raw.Contains('.') || raw.Contains(','))
                {
                    errors.Add(new ValidationErrorDetail(
                        "matrix", r, c, "EMPTY_OR_NON_INTEGER",
                        $"Ô [Hàng {r + 1}, Cột {c + 1}] phải là số nguyên, không được để trống hoặc là số thập phân."
                    ));
                    hasCellError = true;
                }
                else if (val < 0 || val > 255)
                {
                    errors.Add(new ValidationErrorDetail(
                        "matrix", r, c, "PIXEL_OUT_OF_RANGE",
                        $"Ô [Hàng {r + 1}, Cột {c + 1}] có giá trị {val} vượt quá giới hạn cho phép (0 - 255)."
                    ));
                    hasCellError = true;
                }
                else
                {
                    tempMatrix[r][c] = val;
                }
            }
        }

        if (hasRagged)
        {
            errors.Add(new ValidationErrorDetail("matrix", null, null, "RAGGED_MATRIX", "Các hàng trong ma trận phải có cùng số lượng phần tử."));
            return false;
        }

        if (hasCellError)
        {
            return false;
        }

        matrix = tempMatrix;
        return true;
    }

    private static bool ValidateKElement(JsonElement root, List<ValidationErrorDetail> errors, out int k)
    {
        k = 0;
        if (!root.TryGetProperty("k", out var kProp) ||
            kProp.ValueKind != JsonValueKind.Number ||
            !kProp.TryGetInt32(out int kVal) ||
            kProp.GetRawText().Contains('.') ||
            kVal < 1 || kVal > 9)
        {
            errors.Add(new ValidationErrorDetail(
                "k", null, null, "INVALID_K_VALUE",
                "Kích thước cửa sổ k phải là số nguyên trong khoảng [1, 9]."
            ));
            return false;
        }
        k = kVal;
        return true;
    }

    private static bool ValidateMethodElement(JsonElement root, List<ValidationErrorDetail> errors, out string method)
    {
        method = "";
        if (!root.TryGetProperty("method", out var methodProp) ||
            methodProp.ValueKind != JsonValueKind.String)
        {
            errors.Add(new ValidationErrorDetail(
                "method", null, null, "INVALID_METHOD",
                "Phương pháp lọc không hợp lệ. Chỉ chấp nhận MEAN, MEDIAN hoặc PREWITT."
            ));
            return false;
        }

        var m = methodProp.GetString()?.Trim().ToUpperInvariant() ?? "";
        if (m != "MEAN" && m != "MEDIAN" && m != "PREWITT")
        {
            errors.Add(new ValidationErrorDetail(
                "method", null, null, "INVALID_METHOD",
                "Phương pháp lọc không hợp lệ. Chỉ chấp nhận MEAN, MEDIAN hoặc PREWITT."
            ));
            return false;
        }
        method = m;
        return true;
    }

    private static void ValidatePrewittConfigElement(
        JsonElement root,
        int k,
        List<ValidationErrorDetail> errors,
        out PrewittConfigDto? prewittConfig)
    {
        prewittConfig = null;
        bool hasConfig = root.TryGetProperty("prewittConfig", out var configProp) &&
                         configProp.ValueKind == JsonValueKind.Object;

        if (k != 3)
        {
            if (!hasConfig)
            {
                errors.Add(new ValidationErrorDetail(
                    "prewittConfig", null, null, "MISSING_PREWITT_KERNEL",
                    "Với kích thước k ≠ 3, bạn phải tự cung cấp ma trận kernel Gx và Gy tương ứng kích thước k×k."
                ));
                return;
            }

            bool hasKx = configProp.TryGetProperty("kernelX", out var kxProp) && kxProp.ValueKind == JsonValueKind.Array;
            bool hasKy = configProp.TryGetProperty("kernelY", out var kyProp) && kyProp.ValueKind == JsonValueKind.Array;

            if (!hasKx || !hasKy)
            {
                errors.Add(new ValidationErrorDetail(
                    "prewittConfig", null, null, "MISSING_PREWITT_KERNEL",
                    "Với kích thước k ≠ 3, bạn phải tự cung cấp ma trận kernel Gx và Gy tương ứng kích thước k×k."
                ));
                return;
            }

            int[][]? kx = ValidateKernel("kernelX", kxProp, k, errors);
            int[][]? ky = ValidateKernel("kernelY", kyProp, k, errors);

            if (kx != null && ky != null)
            {
                prewittConfig = new PrewittConfigDto(kx, ky);
            }
        }
        else // k == 3
        {
            if (hasConfig)
            {
                int[][]? kx = null;
                int[][]? ky = null;

                if (configProp.TryGetProperty("kernelX", out var kxProp) && kxProp.ValueKind == JsonValueKind.Array)
                {
                    kx = ValidateKernel("kernelX", kxProp, 3, errors);
                }
                if (configProp.TryGetProperty("kernelY", out var kyProp) && kyProp.ValueKind == JsonValueKind.Array)
                {
                    ky = ValidateKernel("kernelY", kyProp, 3, errors);
                }

                if (kx != null || ky != null)
                {
                    prewittConfig = new PrewittConfigDto(kx, ky);
                }
            }
        }
    }

    private static int[][]? ValidateKernel(string kernelName, JsonElement kernelProp, int k, List<ValidationErrorDetail> errors)
    {
        if (kernelProp.ValueKind != JsonValueKind.Array || kernelProp.GetArrayLength() != k)
        {
            errors.Add(new ValidationErrorDetail(
                $"prewittConfig.{kernelName}", null, null, "INVALID_KERNEL_DIMENSIONS",
                $"Ma trận kernel Prewitt phải có kích thước đúng bằng k×k ({k}×{k})."
            ));
            return null;
        }

        int[][] kernel = new int[k][];
        bool hasError = false;

        for (int r = 0; r < k; r++)
        {
            var row = kernelProp[r];
            if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() != k)
            {
                errors.Add(new ValidationErrorDetail(
                    $"prewittConfig.{kernelName}", null, null, "INVALID_KERNEL_DIMENSIONS",
                    $"Ma trận kernel Prewitt phải có kích thước đúng bằng k×k ({k}×{k})."
                ));
                return null;
            }

            kernel[r] = new int[k];
            for (int c = 0; c < k; c++)
            {
                var cell = row[c];
                string raw = cell.GetRawText();
                if (cell.ValueKind != JsonValueKind.Number ||
                    !cell.TryGetInt32(out int val) ||
                    raw.Contains('.') || raw.Contains(','))
                {
                    errors.Add(new ValidationErrorDetail(
                        $"prewittConfig.{kernelName}", r, c, "INVALID_KERNEL_VALUE",
                        "Các giá trị trong kernel Prewitt phải là số nguyên."
                    ));
                    hasError = true;
                }
                else
                {
                    kernel[r][c] = val;
                }
            }
        }

        return hasError ? null : kernel;
    }
}

namespace WebXuLyAnh.Api.Models;

public record FractionDto(
    int Numerator,
    int Denominator,
    string Display
);

public record CellResultDto(
    int Row,
    int Col,
    string DisplayCoordinates,
    FractionDto Exact,
    string Rounded1,
    int RoundedInt,
    StepDetailDto? Step = null
);

public record PaddingInfoDto(
    int PadTop,
    int PadBottom,
    int PadLeft,
    int PadRight
);

public record StepDetailDto(
    int[][] Window,
    MeanStepDto? Mean = null,
    MedianStepDto? Median = null,
    PrewittStepDto? Prewitt = null
);

public record MeanStepDto(
    int Sum,
    int Count,
    string Fraction,
    string ReducedFraction,
    string Formula
);

public record MedianStepDto(
    int[] RawElements,
    int[] SortedElements,
    int[] PickedIndices,
    int[] PickedValues,
    bool IsEven,
    string Formula
);

public record PrewittStepDto(
    int[][] KernelXOriginal,
    int[][] KernelYOriginal,
    int[][] KernelXFlipped,
    int[][] KernelYFlipped,
    int Gx,
    int Gy,
    int AbsGx,
    int AbsGy,
    int G,
    string FormulaGx,
    string FormulaGy,
    string FormulaG
);

public record MatrixSizeDto(
    int Rows,
    int Cols
);

public record WarningDto(
    string Code,
    string Message
);

public record FilterResponse(
    bool Success,
    string Method,
    int K,
    MatrixSizeDto MatrixSize,
    PaddingInfoDto Padding,
    List<WarningDto> Warnings,
    CellResultDto[][] Results
);

public record BatchFilterResponse(
    bool Success,
    MatrixSizeDto MatrixSize,
    int K,
    List<WarningDto> Warnings,
    Dictionary<string, FilterResponse?> Data
);

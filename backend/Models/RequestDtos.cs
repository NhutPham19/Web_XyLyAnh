namespace WebXuLyAnh.Api.Models;

public record MatrixFilterRequest(
    int[][] Matrix,
    int K,
    string Method,
    PrewittConfigDto? PrewittConfig = null,
    FilterOptionsDto? Options = null
);

public record PrewittConfigDto(
    int[][]? KernelX,
    int[][]? KernelY
);

public record FilterOptionsDto(
    bool IncludeSteps = true
);

public record BatchFilterRequest(
    int[][] Matrix,
    int K,
    List<string> Methods,
    PrewittConfigDto? PrewittConfig = null,
    FilterOptionsDto? Options = null
);

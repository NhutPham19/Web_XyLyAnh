namespace WebXuLyAnh.Api.Models;

public record ValidationErrorDetail(
    string Field,
    int? Row,
    int? Col,
    string Code,
    string Message
);

public record ErrorResponse(
    bool Success,
    int StatusCode,
    string ErrorSummary,
    List<ValidationErrorDetail> Errors
);

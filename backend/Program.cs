using System.Text.Json;
using System.Text.Json.Serialization;
using WebXuLyAnh.Api.Models;
using WebXuLyAnh.Api.Services;
using WebXuLyAnh.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.WriteIndented = false;
});

builder.Services.AddSingleton<IFilterService, FilterService>();

var app = builder.Build();

// 2. Configure HTTP Pipeline
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

// 3. API Endpoints
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.MapPost("/api/filter", (JsonElement json, IFilterService filterService) =>
{
    if (!MatrixValidator.ValidateFilterJson(json, out var request, out var warnings, out var errors))
    {
        var errorResponse = new ErrorResponse(
            Success: false,
            StatusCode: 422,
            ErrorSummary: "Dữ liệu ma trận đầu vào không hợp lệ. Vui lòng kiểm tra lại các ô bị đánh dấu.",
            Errors: errors
        );
        return Results.Json(errorResponse, statusCode: 422);
    }

    var response = filterService.ApplyFilter(request!, warnings);
    return Results.Ok(response);
});

app.MapPost("/api/filter/batch", (JsonElement json, IFilterService filterService) =>
{
    if (!MatrixValidator.ValidateBatchFilterJson(json, out var request, out var warnings, out var errors))
    {
        var errorResponse = new ErrorResponse(
            Success: false,
            StatusCode: 422,
            ErrorSummary: "Dữ liệu ma trận đầu vào không hợp lệ. Vui lòng kiểm tra lại các ô bị đánh dấu.",
            Errors: errors
        );
        return Results.Json(errorResponse, statusCode: 422);
    }

    var response = filterService.ApplyBatchFilter(request!, warnings);
    return Results.Ok(response);
});

// 4. Client SPA Fallback
app.MapFallbackToFile("index.html");

app.Run();

// Make Program public for WebApplicationFactory in integration tests
public partial class Program { }

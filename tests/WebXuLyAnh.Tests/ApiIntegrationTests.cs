using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WebXuLyAnh.Api.Models;
using Xunit;

namespace WebXuLyAnh.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_Returns_200OK()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", content.GetProperty("status").GetString());
    }

    [Fact]
    public async Task PostFilter_ValidRequest_Returns_200OK()
    {
        var payload = new
        {
            matrix = new int[][] {
                [10, 20, 30],
                [40, 50, 60],
                [70, 80, 90]
            },
            k = 3,
            method = "MEAN",
            options = new { includeSteps = true }
        };

        var response = await _client.PostAsJsonAsync("/api/filter", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var filterResponse = await response.Content.ReadFromJsonAsync<FilterResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(filterResponse);
        Assert.True(filterResponse.Success);
        Assert.Equal("MEAN", filterResponse.Method);
        Assert.Equal("13.3", filterResponse.Results[0][0].Rounded1);
    }

    [Fact]
    public async Task PostFilter_InvalidRequest_Returns_422WithStandardErrorSchema()
    {
        var payload = new
        {
            matrix = new object[][] {
                [10, 300], // 300 > 255
                [20, "abc"] // non integer
            },
            k = 3,
            method = "MEAN"
        };

        var response = await _client.PostAsJsonAsync("/api/filter", payload);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.Equal(422, errorResponse.StatusCode);
        Assert.Contains(errorResponse.Errors, e => e.Code == "PIXEL_OUT_OF_RANGE");
        Assert.Contains(errorResponse.Errors, e => e.Code == "EMPTY_OR_NON_INTEGER");
    }

    [Fact]
    public async Task PostFilterBatch_ValidRequest_Returns_200OKWithAllMethods()
    {
        var payload = new
        {
            matrix = new int[][] {
                [10, 20, 30],
                [40, 50, 60],
                [70, 80, 90]
            },
            k = 3,
            methods = new string[] { "MEAN", "MEDIAN", "PREWITT" }
        };

        var response = await _client.PostAsJsonAsync("/api/filter/batch", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var batchResponse = await response.Content.ReadFromJsonAsync<BatchFilterResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(batchResponse);
        Assert.True(batchResponse.Success);
        Assert.True(batchResponse.Data.ContainsKey("mean"));
        Assert.True(batchResponse.Data.ContainsKey("median"));
        Assert.True(batchResponse.Data.ContainsKey("prewitt"));
    }
}

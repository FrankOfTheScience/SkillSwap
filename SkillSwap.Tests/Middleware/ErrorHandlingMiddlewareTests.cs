using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SkillSwap.Api.Middleware;
using System.Net;
using System.Text.Json;

namespace SkillSwap.Tests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        var context = new DefaultHttpContext();
        var called = false;
        RequestDelegate next = ctx =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var middleware = new ErrorHandlingMiddleware(next);

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_ReturnsErrorResponse()
    {
        var context = new DefaultHttpContext();
        var stream = new MemoryStream();
        context.Response.Body = stream;

        var exceptionMessage = "Test exception";
        RequestDelegate next = ctx => throw new Exception(exceptionMessage);

        var middleware = new ErrorHandlingMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        stream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(stream).ReadToEndAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseBody);

        responseObj.GetProperty("error").GetString().Should().Be(exceptionMessage);
    }
}

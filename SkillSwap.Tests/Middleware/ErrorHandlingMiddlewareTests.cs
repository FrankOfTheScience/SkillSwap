using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
        var logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();

        var middleware = new ErrorHandlingMiddleware(next, logger);

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
        var logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();

        var middleware = new ErrorHandlingMiddleware(next, logger);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        stream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(stream).ReadToEndAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Updated to match the new ErrorResponse structure
        responseObj.GetProperty("message").GetString().Should().Be("Something went wrong on our end. Please try again later.");
        responseObj.GetProperty("type").GetString().Should().Be("error");
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_ReturnsBadRequest()
    {
        var context = new DefaultHttpContext();
        var stream = new MemoryStream();
        context.Response.Body = stream;

        RequestDelegate next = ctx => throw new ArgumentException("Invalid argument");
        var logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();

        var middleware = new ErrorHandlingMiddleware(next, logger);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        stream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(stream).ReadToEndAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseBody);

        responseObj.GetProperty("message").GetString().Should().Be("Invalid request. Please check your input and try again.");
        responseObj.GetProperty("type").GetString().Should().Be("error");
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ReturnsNotFound()
    {
        var context = new DefaultHttpContext();
        var stream = new MemoryStream();
        context.Response.Body = stream;

        RequestDelegate next = ctx => throw new KeyNotFoundException("Resource not found");
        var logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();

        var middleware = new ErrorHandlingMiddleware(next, logger);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/json");

        stream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(stream).ReadToEndAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseBody);

        responseObj.GetProperty("message").GetString().Should().Be("The requested resource was not found.");
        responseObj.GetProperty("type").GetString().Should().Be("error");
    }
}

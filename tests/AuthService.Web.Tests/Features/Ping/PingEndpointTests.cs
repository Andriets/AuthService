using AuthService.Web.Features.Ping;
using Microsoft.AspNetCore.Http.HttpResults;
using NUnit.Framework;

namespace AuthService.Web.Tests.Features.Ping;

[TestFixture]
public class PingEndpointTests
{
    [Test]
    public void Handler_ReturnsOkResult()
    {
        var result = PingEndpoint.Handler(TimeProvider.System);

        Assert.That(result, Is.InstanceOf<Ok<PingResponse>>());
    }

    [Test]
    public void Handler_StatusIsOk()
    {
        var result = PingEndpoint.Handler(new FixedTimeProvider(DateTimeOffset.UtcNow));

        Assert.That(result.Value!.Status, Is.EqualTo("ok"));
    }

    [Test]
    public void Handler_TimestampMatchesTimeProvider()
    {
        var expected = new DateTimeOffset(2026, 6, 26, 12, 0, 0, TimeSpan.Zero);

        var result = PingEndpoint.Handler(new FixedTimeProvider(expected));

        Assert.That(result.Value!.Timestamp, Is.EqualTo(expected));
    }
}

file sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}

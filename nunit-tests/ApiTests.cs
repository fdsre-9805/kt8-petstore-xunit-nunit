using System.Text.Json;
using NUnit.Framework;
namespace PetstoreTests;
[NonParallelizable]
public abstract class ApiTests
{
    protected PetstoreApi Api = null!;
    [SetUp]
    public void Setup() => Api = new();
    [TearDown]
    public async Task Cleanup() => await Api.DisposeAsync();
    protected static void Status(int expected, (int Status, JsonElement Body) result)
        => Assert.That(result.Status, Is.EqualTo(expected), result.Body.ToString());
    protected static void Check(bool condition) => Assert.That(condition, Is.True);
}

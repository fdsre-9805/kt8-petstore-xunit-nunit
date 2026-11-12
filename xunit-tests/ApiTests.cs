using System.Text.Json;
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace PetstoreTests;
public abstract class ApiTests : IAsyncLifetime
{
    protected PetstoreApi Api = new();
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await Api.DisposeAsync();
    protected static void Status(int expected, (int Status, JsonElement Body) result)
        => Assert.True(expected == result.Status, $"Ожидалось {expected}, получено {result.Status}: {result.Body}");
    protected static void Check(bool condition) => Assert.True(condition);
}

using System.Text.Json;
using NUnit.Framework;
namespace PetstoreTests;

public class StoreTests : ApiTests
{
    [Test]
    public async Task Inventory()
    {
        var result = await Api.Send("GET", "store/inventory");
        Status(200, result);
        Check(result.Body.ValueKind == JsonValueKind.Object);
        Check(result.Body.EnumerateObject().All(x => x.Value.TryGetInt32(out var count) && count >= 0));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CreateOrder(bool negative)
    {
        var data = await Api.OrderData();
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("POST", "store/order", data);
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"store/order/{data["id"]}");
            Status(200, found);
            Check(found.Body.GetProperty("petId").GetInt64() == (long)data["petId"]);
            Check(found.Body.GetProperty("quantity").GetInt32() == 2);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task GetOrder(bool negative)
    {
        var data = await Api.OrderData();
        await Api.Create("store/order", data);
        var result = await Api.Send("GET", "store/order/" + (negative ? "wrong_id" : data["id"]));
        Status(negative ? 400 : 200, result);
        if (!negative) Check(result.Body.GetProperty("petId").GetInt64() == (long)data["petId"]);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DeleteOrder(bool negative)
    {
        var data = await Api.OrderData();
        await Api.Create("store/order", data);
        var result = await Api.Send("DELETE", "store/order/" + (negative ? "wrong_id" : data["id"]));
        Status(negative ? 400 : 200, result);
        if (!negative) Status(404, await Api.Send("GET", $"store/order/{data["id"]}"));
    }

    [Test]
    public async Task GetMissingOrder()
    {
        var data = await Api.OrderData();
        await Api.Create("store/order", data);
        var path = $"store/order/{data["id"]}";
        Status(200, await Api.Send("DELETE", path));
        Status(404, await Api.Send("GET", path));
        Status(404, await Api.Send("GET", path));
    }

    [Test]
    public async Task DeleteMissingOrder()
    {
        var data = await Api.OrderData();
        await Api.Create("store/order", data);
        var path = $"store/order/{data["id"]}";
        Status(200, await Api.Send("DELETE", path));
        Status(404, await Api.Send("GET", path));
        Status(404, await Api.Send("DELETE", path));
    }

}

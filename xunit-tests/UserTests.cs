using System.Text.Json;
using Xunit;
namespace PetstoreTests;

public class UserTests : ApiTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateUser(bool negative)
    {
        var data = Api.UserData();
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("POST", "user", data);
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"user/{data["username"]}");
            Status(200, found);
            Check(found.Body.GetProperty("email").GetString() == (string)data["email"]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateUsersList(bool negative)
    {
        var data = Api.UserData();
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("POST", "user/createWithList", new[] { data });
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"user/{data["username"]}");
            Status(200, found);
            Check(found.Body.GetProperty("email").GetString() == (string)data["email"]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateUsersArray(bool negative)
    {
        var data = Api.UserData();
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("POST", "user/createWithArray", new[] { data });
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"user/{data["username"]}");
            Status(200, found);
            Check(found.Body.GetProperty("email").GetString() == (string)data["email"]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetUser(bool negative)
    {
        var data = Api.UserData();
        await Api.Create("user", data);
        var name = negative ? "missing_" + Guid.NewGuid().ToString("N") : (string)data["username"];
        var result = await Api.Send("GET", "user/" + name);
        Status(negative ? 404 : 200, result);
        if (!negative) Check(result.Body.GetProperty("email").GetString() == (string)data["email"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateUser(bool negative)
    {
        var data = Api.UserData();
        await Api.Create("user", data);
        var name = (string)data["username"];
        data["email"] = "updated@example.com";
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("PUT", "user/" + name, data);
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", "user/" + name);
            Status(200, found);
            Check(found.Body.GetProperty("email").GetString() == "updated@example.com");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteUser(bool negative)
    {
        var data = Api.UserData();
        await Api.Create("user", data);
        var name = negative ? "missing_" + Guid.NewGuid().ToString("N") : (string)data["username"];
        var result = await Api.Send("DELETE", "user/" + name);
        Status(negative ? 404 : 200, result);
        if (!negative) Status(404, await Api.Send("GET", "user/" + name));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Login(bool negative)
    {
        var data = Api.UserData();
        await Api.Create("user", data);
        var password = negative ? "wrong_password" : (string)data["password"];
        var result = await Api.Send("GET", $"user/login?username={data["username"]}&password={password}");
        Status(negative ? 400 : 200, result);
        if (!negative) Check(result.Body.GetProperty("message").GetString()!.Contains("logged in user session"));
    }

    [Fact]
    public async Task Logout()
    {
        var result = await Api.Send("GET", "user/logout");
        Status(200, result);
        Check(result.Body.GetProperty("message").ValueKind == JsonValueKind.String);
    }

}

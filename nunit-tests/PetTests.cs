using System.Text.Json;
using NUnit.Framework;
namespace PetstoreTests;

public class PetTests : ApiTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task CreatePet(bool negative)
    {
        var data = Api.PetData();
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("POST", "pet", data);
        Status(negative ? 405 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"pet/{data["id"]}");
            Status(200, found);
            Check(found.Body.GetProperty("name").GetString() == (string)data["name"]);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task UpdatePet(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        data["status"] = "sold";
        if (negative) data["id"] = "wrong_id";
        var result = await Api.Send("PUT", "pet", data);
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"pet/{data["id"]}");
            Status(200, found);
            Check(found.Body.GetProperty("status").GetString() == "sold");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FindByStatus(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        var result = await Api.Send("GET", "pet/findByStatus?status=" + (negative ? "wrong_status" : "available"));
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            Check(result.Body.ValueKind == JsonValueKind.Array);
            Check(result.Body.EnumerateArray().Any(x => x.GetProperty("id").GetInt64() == (long)data["id"]));
            Check(result.Body.EnumerateArray().All(x => x.GetProperty("status").GetString() == "available"));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FindByTags(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        var token = ((string)data["name"])[4..];
        var result = await Api.Send("GET", "pet/findByTags" + (negative ? "" : "?tags=" + token));
        Status(negative ? 400 : 200, result);
        if (!negative)
        {
            Check(result.Body.ValueKind == JsonValueKind.Array);
            Check(result.Body.EnumerateArray().Any(x => x.GetProperty("id").GetInt64() == (long)data["id"]));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task GetPet(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        var result = await Api.Send("GET", "pet/" + (negative ? "wrong_id" : data["id"]));
        Status(negative ? 400 : 200, result);
        if (!negative) Check(result.Body.GetProperty("name").GetString() == (string)data["name"]);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task UpdatePetForm(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["status"] = "sold" });
        var result = await Api.Send("POST", "pet/" + (negative ? "wrong_id" : data["id"]), content: form);
        Status(negative ? 405 : 200, result);
        if (!negative)
        {
            var found = await Api.Send("GET", $"pet/{data["id"]}");
            Status(200, found);
            Check(found.Body.GetProperty("status").GetString() == "sold");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task DeletePet(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        var result = await Api.Send("DELETE", "pet/" + (negative ? "wrong_id" : data["id"]));
        Status(negative ? 400 : 200, result);
        if (!negative) Status(404, await Api.Send("GET", $"pet/{data["id"]}"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task UploadImage(bool negative)
    {
        var data = Api.PetData();
        await Api.Create("pet", data);
        using var form = new MultipartFormDataContent();
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGP4z8DwHwAFAAH/iZk9HQAAAABJRU5ErkJggg==");
        var image = new ByteArrayContent(bytes);
        image.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form.Add(image, "file", "pet.png");
        var result = await Api.Send("POST", "pet/" + (negative ? "wrong_id" : data["id"]) + "/uploadImage", content: form);
        Status(negative ? 400 : 200, result);
        if (!negative) Check(result.Body.GetProperty("message").GetString()!.Contains("File uploaded"));
    }

}

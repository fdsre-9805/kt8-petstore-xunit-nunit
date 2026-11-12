using System.Net.Http.Json;
using System.Text.Json;

namespace PetstoreTests;

public sealed class PetstoreApi : IAsyncDisposable
{
    private readonly HttpClient client = new()
    {
        BaseAddress = new Uri("https://petstore.swagger.io/v2/"),
        Timeout = TimeSpan.FromSeconds(20)
    };
    private readonly List<(string Path, long Id, string Key, string Value)> records = [];

    public PetstoreApi()
    {
        client.DefaultRequestHeaders.Add("api_key", "special-key");
    }

    public async Task<(int Status, JsonElement Body)> Send(
        string method, string path, object? body = null, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Content = content ?? (body is null ? null : JsonContent.Create(body));
        using var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{method} {path} -> {(int)response.StatusCode}");
        JsonElement data;
        try { data = JsonDocument.Parse(text).RootElement.Clone(); }
        catch (JsonException) { data = JsonSerializer.SerializeToElement(text); }
        return ((int)response.StatusCode, data);
    }

    public Dictionary<string, object> PetData()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var id = Convert.ToInt64(token, 16);
        var name = "kt8_" + token;
        records.Add(($"pet/{id}", id, "name", name));
        return new() { ["id"] = id, ["name"] = name,
            ["photoUrls"] = Array.Empty<string>(), ["status"] = "available",
            ["tags"] = new[] { new { id = 1, name = token } } };
    }

    public Dictionary<string, object> UserData()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var id = Convert.ToInt64(token, 16);
        var name = "kt8_" + token;
        records.Add(($"user/{name}", id, "username", name));
        return new() { ["id"] = id, ["username"] = name,
            ["firstName"] = "Test", ["lastName"] = "Student",
            ["email"] = token + "@example.com", ["password"] = "Kt8Password",
            ["phone"] = "12345", ["userStatus"] = 0 };
    }

    public async Task<Dictionary<string, object>> OrderData()
    {
        var pet = PetData();
        await Create("pet", pet);
        for (var id = 1; id <= 10; id++)
        {
            var found = await Send("GET", $"store/order/{id}");
            if (found.Status == 404)
            {
                var petId = (long)pet["id"];
                records.Add(($"store/order/{id}", id, "petId", petId.ToString()));
                return new() { ["id"] = id, ["petId"] = petId,
                    ["quantity"] = 2, ["status"] = "placed", ["complete"] = false };
            }
            if (found.Status != 200) throw new InvalidOperationException(found.ToString());
        }
        throw new InvalidOperationException("Все ID заказов 1–10 заняты");
    }

    public async Task Create(string path, Dictionary<string, object> data)
    {
        var response = await Send("POST", path, data);
        if (response.Status != 200)
            throw new InvalidOperationException($"Подготовка: {response.Status} {response.Body}");
        var record = path == "user" ? $"user/{data["username"]}"
            : path == "pet" ? $"pet/{data["id"]}" : $"store/order/{data["id"]}";
        var found = await Send("GET", record);
        if (found.Status != 200 || found.Body.GetProperty("id").GetInt64() != Convert.ToInt64(data["id"]))
            throw new InvalidOperationException($"Запись не сохранена: {found}");
    }

    public async ValueTask DisposeAsync()
    {
        var errors = new List<Exception>();
        foreach (var item in records.AsEnumerable().Reverse())
        {
            try
            {
                var found = await Send("GET", item.Path);
                if (found.Status == 404) continue;
                if (found.Status != 200) throw new InvalidOperationException(found.ToString());
                if (found.Body.GetProperty("id").GetInt64() != item.Id ||
                    found.Body.GetProperty(item.Key).ToString() != item.Value)
                    throw new InvalidOperationException("Запись другого теста: удаление отменено");
                var deleted = await Send("DELETE", item.Path);
                if (deleted.Status is not (200 or 404))
                    throw new InvalidOperationException(deleted.ToString());
                var check = await Send("GET", item.Path);
                if (check.Status != 404) throw new InvalidOperationException("Запись не удалена");
            }
            catch (Exception error) { errors.Add(error); }
        }
        client.Dispose();
        if (errors.Count > 0) throw new AggregateException("Ошибки очистки", errors);
    }
}

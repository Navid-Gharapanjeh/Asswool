using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<MemoryDbContext>();
var app = builder.Build();

// API Endpoints

// Ollama Configuration
const string ollamaUrl = "http://localhost:11434/api/generate";

// Chat endpoint
app.MapPost("/chat", async (string userMessage, MemoryDbContext dbContext) =>
{
    // Fetch memory
    var memories = dbContext.Memories.ToList();
    var memoryContext = string.Join("\n", memories.Select(m => $"{m.Key}: {m.Value}"));

    // Send prompt to Ollama
    var prompt = $"Memory:{memoryContext} User: {userMessage}AI:";
    var response = await PostToOllama(ollamaUrl, "llama3.2", prompt);

    return Results.Ok(response);
});

// Save memory
app.MapPost("/memory", async (string key, string value, MemoryDbContext dbContext) =>
{
    // Save to database
    var memory = new Memory { Key = key, Value = value };
    dbContext.Memories.Add(memory);
    await dbContext.SaveChangesAsync();

    return Results.Ok("Memory saved.");
});

app.MapGet("/", () => "Hello World!");

// Utility to interact with Ollama
static async Task<string> PostToOllama(string url, string model, string prompt)
{
    using var client = new HttpClient();
    var content = new StringContent(JsonConvert.SerializeObject(new { model, prompt }), System.Text.Encoding.UTF8, "application/json");
    var response = await client.PostAsync(url, content);

    var contentStream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(contentStream);
    var fullResponse = new StringBuilder();

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(line);
        if (jsonResponse.TryGetProperty("response", out var fragment))
        {
            fullResponse.Append(fragment.GetString());
        }

        if (jsonResponse.TryGetProperty("done", out var done) && done.GetBoolean())
        {
            break; // Exit when the response is complete
        }
    }

    return fullResponse.ToString();


    //var responseBody = await response.Content.ReadAsStringAsync();
    //return JsonConvert.DeserializeObject<dynamic>(responseBody)?["response"]?.ToString() ?? "No response.";
}


app.Run();

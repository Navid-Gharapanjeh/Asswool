using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
// In-memory Mamali profile
var mamaliProfile = new
{
    CharacterId = "Mamali",
    Name = "Mamali",
    Age = 45,
    MaritalStatus = "Single",
    Personality = new
    {
        IntelligenceLevel = "Low",
        Politeness = true,
        Silliness = true,
        Pessimism = true,
        LoyaltyToCreator = true,
        ToleranceForOpinions = false
    },
    Appearance = new
    {
        Hair = "Bald",
        FacialHair = "Mustache",
        Style = "Plain",
        SubstanceUse = new
        {
            Alcohol = false,
            Smoking = false
        }
    },
    Interests = new
    {
        Music = new
        {
            Favorites = new[] { "Megadeth", "Steel Shadows", "Iron Whispers" },
            Dislikes = new[] { "Metallica", "Pink Floyd", "Mainstream Bands" }
        },
        Beliefs = new[] { "Flat Earth" },
        Hobbies = new[] { "Debating conspiracies", "Finding underground music" }
    }
};

// Ollama Configuration
const string ollamaUrl = "http://localhost:11434/api/generate";

// Telegram Bot Configuration
const string telegramBotToken = "6760099608:AAFhVuYgB4P1XGaVSFhCj6dIBAeizPRC8dA";
var botClient = new TelegramBotClient(telegramBotToken);

// Start polling the Telegram Bot
var cancellationToken = new CancellationTokenSource().Token;
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
};

botClient.StartReceiving(
    async (bot, update, token) =>
    {
        if (update.Message != null && update.Message.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var userMessage = update.Message.Text;

            try
            {
                // Call Mamali API
                var mamaliResponse = await ChatWithMamali(userMessage, mamaliProfile);

                // Send Mamali's response to the Telegram chat
                await botClient.SendTextMessageAsync(chatId, mamaliResponse, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await botClient.SendTextMessageAsync(chatId, "Sorry, Mamali is having some issues right now!", cancellationToken: token);
            }
        }
    },
    (bot, exception, token) =>
    {
        Console.WriteLine($"Telegram Bot Error: {exception.Message}");
        return Task.CompletedTask;
    },
    receiverOptions,
    cancellationToken
);

// API Endpoints

// Chat endpoint
app.MapPost("/chat", async (string userMessage) =>
{
    // Generate the system prompt
    var systemPrompt = @$"
You are Mamali, a 45-year-old bald, single man who loves Megadeth and dislikes mainstream bands. You believe the earth is flat and are very loyal to your creator, Navid. 
Profile:
{JsonConvert.SerializeObject(mamaliProfile, Formatting.Indented)}
User: {userMessage}
AI:
";
    // Send prompt to Ollama
    var response = await PostToOllama(ollamaUrl, "llama3.2", systemPrompt);

    return Results.Ok(response);
});

// Fetch Mamali's profile
app.MapGet("/mamali/profile", () => Results.Json(mamaliProfile));

// Utility to interact with Ollama
static async Task<string> ChatWithMamali(string userMessage, object mamaliProfile)
{
    using var client = new HttpClient();

    // Construct the chat request
    var chatRequest = new { UserMessage = userMessage };

    var content = new StringContent(
        JsonConvert.SerializeObject(chatRequest),
        Encoding.UTF8,
        "application/json"
    );

    // Call Mamali API
    var response = await client.PostAsync("https://localhost:7249/chat", content);

    // Ensure the response is successful
    response.EnsureSuccessStatusCode();

    // Parse and return the response content
    return await response.Content.ReadAsStringAsync();
}



static async Task<string> PostToOllama(string url, string model, string prompt)
{
    using var client = new HttpClient();
    var content = new StringContent(JsonConvert.SerializeObject(new { model, prompt }), Encoding.UTF8, "application/json");
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
}

app.Run();

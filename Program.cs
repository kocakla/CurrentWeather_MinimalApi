using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/weather", async (string city) =>
{
    using var httpClient = new HttpClient();

    // 1. Şehir adını koordinata çevir
    var geoUrl = $"https://nominatim.openstreetmap.org/search?q={city}&format=json&limit=1";
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0");
    var geoResponse = await httpClient.GetStringAsync(geoUrl);
    var geoData = JsonSerializer.Deserialize<List<GeoResult>>(geoResponse);

    if (geoData == null || geoData.Count == 0)
        return Results.NotFound($"City '{city}' not found.");

    var lat = geoData[0].Lat;
    var lon = geoData[0].Lon;

    // 2. Open-Meteo'dan hava durumu al
    var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
    var weatherResponse = await httpClient.GetStringAsync(weatherUrl);

    return Results.Content(weatherResponse, "application/json");
});

app.UseDefaultFiles(); // index.html varsayılan olarak yüklensin
app.UseStaticFiles();  

app.Run();

// JSON model: koordinat yanıtı
record GeoResult(
    [property: JsonPropertyName("lat")] string Lat,
    [property: JsonPropertyName("lon")] string Lon
);

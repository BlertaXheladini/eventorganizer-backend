using Newtonsoft.Json.Linq;

public class WeatherService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public WeatherService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetWeather(string city, DateTime date)
    {
        string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric";
        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);

        var targetDate = date.Date.AddHours(12);
        string weather = json["list"]
            .OrderBy(item => Math.Abs((DateTime.Parse(item["dt_txt"].ToString()) - targetDate).TotalHours))
            .Select(item => item["weather"][0]["main"].ToString())
            .FirstOrDefault();

        return weather ?? "Unknown";
    }
}

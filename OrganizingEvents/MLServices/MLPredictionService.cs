using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class MLPredictionService
{
    private static readonly string dataPath = @"C:\Users\Lenovo\Desktop\event_data_europe_5000.csv";
    private static readonly string modelPath = @"C:\Users\Lenovo\Desktop\saved-model.zip";

    public static async Task<float> MakePrediction(int eventId, string city, DateTime predictionDate)
    {
        var context = new MLContext();
        var model = context.Model.Load(modelPath, out var schema);

        var eventData = await GetEventData(eventId, city, predictionDate);
        if (eventData == null)
        {
            Console.WriteLine($"Nuk u gjet eventi me ID: {eventId}");
            return -1;
        }

        var predictionEngine = context.Model.CreatePredictionEngine<EventData, PredictionResult>(model);
        var result = predictionEngine.Predict(eventData);

        return result.PredictedParticipants;
    }

    private static async Task<EventData> GetEventData(int eventId, string city, DateTime predictionDate)
    {
        var lines = File.ReadAllLines(dataPath).Skip(1);

        var eventLine = lines.Select(line => line.Split(','))
                             .FirstOrDefault(cols => int.Parse(cols[0]) == eventId);

        if (eventLine == null)
            return null;

        var weatherService = new WeatherService("325e9084d0380620bb2a3e40059b2d1c");

        return new EventData
        {
            EventID = int.Parse(eventLine[0]),
            Price = float.Parse(eventLine[1]),
            CategoryId = int.Parse(eventLine[2]),
            ThemeId = int.Parse(eventLine[3]),
            Date = eventLine[4],
            City = city,
            WeatherCondition = await weatherService.GetWeather(city, predictionDate),
            Season = GetSeason(predictionDate),
            Month = predictionDate.Month,
            DayOfWeek = (int)predictionDate.DayOfWeek,
            IsWeekend = predictionDate.DayOfWeek == DayOfWeek.Saturday || predictionDate.DayOfWeek == DayOfWeek.Sunday,
            TotalParticipants = 0 // default, pasi kjo është vlera që duam ta parashikojmë
        };
    }

    private static string GetSeason(DateTime date)
    {
        int month = date.Month;
        if (month == 12 || month <= 2)
            return "Winter";
        if (month <= 5)
            return "Spring";
        if (month <= 8)
            return "Summer";
        return "Autumn";
    }
}


using Microsoft.ML;
using Microsoft.ML.Data;
public class EventData

{
    [LoadColumn(0)] public int EventID { get; set; }
    [LoadColumn(1)] public float Price { get; set; }
    [LoadColumn(2)] public int CategoryId { get; set; }
    [LoadColumn(3)] public int ThemeId { get; set; }
    [LoadColumn(4)] public string Date { get; set; }
    [LoadColumn(5)] public string City { get; set; }
    [LoadColumn(6)] public string WeatherCondition { get; set; }
    [LoadColumn(7)] public string Season { get; set; }
    [LoadColumn(8)] public float Month { get; set; }
    [LoadColumn(9)] public float DayOfWeek { get; set; }
    [LoadColumn(10)] public bool IsWeekend { get; set; }

    [LoadColumn(11), ColumnName("Label")]
    public float TotalParticipants { get; set; }
}

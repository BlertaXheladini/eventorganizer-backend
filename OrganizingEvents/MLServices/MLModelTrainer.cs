using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Linq;
using static Microsoft.ML.Transforms.OneHotEncodingEstimator;

public class MLModelTrainer
{
    public static void TrainAndSaveModel()
    {
        var context = new MLContext();
        var dataPath = @"C:\Users\Lenovo\Desktop\event_data_europe_5000.csv";

        var dataView = context.Data.LoadFromTextFile<EventData>(
            dataPath,
            separatorChar: ',',
            hasHeader: true
        );

        var pipeline = context.Transforms.Categorical.OneHotEncoding(new[]
{
    new InputOutputColumnPair("CategoryEncoded", "CategoryId"),
    new InputOutputColumnPair("ThemeEncoded", "ThemeId"),
    new InputOutputColumnPair("SeasonEncoded", "Season"),
    new InputOutputColumnPair("WeatherEncoded", "WeatherCondition"),
    new InputOutputColumnPair("CityEncoded", "City"),
})
// Shto konvertimin për kolonën IsWeekend këtu
.Append(context.Transforms.Conversion.ConvertType("IsWeekendNumeric", "IsWeekend", DataKind.Single))
.Append(context.Transforms.Concatenate("Features",
    "CategoryEncoded", "ThemeEncoded", "CityEncoded",
    "Price", "Month", "DayOfWeek", "IsWeekendNumeric",
    "SeasonEncoded", "WeatherEncoded"))
.Append(context.Regression.Trainers.FastTree(
    new Microsoft.ML.Trainers.FastTree.FastTreeRegressionTrainer.Options
    {
        NumberOfLeaves = 50,
        MinimumExampleCountPerLeaf = 5,
        NumberOfTrees = 200,
        LearningRate = 0.05f,
        LabelColumnName = "Label",
        FeatureColumnName = "Features"
    }
));


        var model = pipeline.Fit(dataView);

        context.Model.Save(model, dataView.Schema, @"C:\Users\Lenovo\Desktop\saved-model.zip");

        Console.WriteLine("Modeli u trajnua dhe u ruajt me sukses!");
    }


    
    // Funksion për të marrë stinën nga data
    public static string GetSeason(DateTime date) // Tani pranon DateTime
    {
        int month = date.Month;

        if (month == 12 || month == 1 || month == 2)
            return "Winter";
        else if (month >= 3 && month <= 5)
            return "Spring";
        else if (month >= 6 && month <= 8)
            return "Summer";
        else
            return "Autumn";
    }


}

// Klasa për të transformuar stinën
public class TransformedData
{
    public string Season { get; set; }
}

// Klasa për të transformuar EventData
public class TransformedEventData
{
    public int EventID { get; set; }
    public float Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryIdText { get; set; }
    public int ThemeId { get; set; }
    public string ThemeIdText { get; set; }
    public string Date { get; set; }
    public string WeatherCondition { get; set; }
    public float TotalParticipants { get; set; }


}

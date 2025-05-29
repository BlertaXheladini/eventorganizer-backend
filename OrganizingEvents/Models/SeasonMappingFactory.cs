using Microsoft.ML;
using Microsoft.ML.Transforms;
using System;

public class SeasonMappingFactory
{
    public static Action<TransformedEventData, TransformedData> GetMapping()
    {
        return (input, output) =>
        {
            var date = DateTime.Parse(input.Date);
            int month = date.Month;

            if (month == 12 || month == 1 || month == 2)
                output.Season = "Winter";
            else if (month >= 3 && month <= 5)
                output.Season = "Spring";
            else if (month >= 6 && month <= 8)
                output.Season = "Summer";
            else
                output.Season = "Autumn";
        };
    }
}

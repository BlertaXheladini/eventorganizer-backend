using Microsoft.ML.Data;

public class PredictionResult
{
    [ColumnName("Score")]
    public float PredictedParticipantsFloat { get; set; }

    public int PredictedParticipants => (int)Math.Round(PredictedParticipantsFloat);
}


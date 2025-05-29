using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    [HttpGet("train")]
    public IActionResult Train()
    {
        MLModelTrainer.TrainAndSaveModel();
        return Ok("Modeli u trajnua me sukses!");
    }

    [HttpGet("predict")]
    public async Task<IActionResult> Predict(int eventId, string city, DateTime date)
    {
        float participants = await MLPredictionService.MakePrediction(eventId, city, date);
        if (participants < 0)
            return NotFound("Eventi nuk u gjet.");

        return Ok(new { Participants = participants });
    }
}

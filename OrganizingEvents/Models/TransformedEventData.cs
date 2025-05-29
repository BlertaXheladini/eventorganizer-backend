namespace OrganizingEvents.Models
{
    public class TransformedEventData
    {
        public int EventID { get; set; }
        public float Price { get; set; }
        public string CategoryIdText { get; set; }
        public string ThemeIdText { get; set; }
        public string Date { get; set; }
        public string WeatherCondition { get; set; }
        public float TotalParticipants { get; set; }
    }
}

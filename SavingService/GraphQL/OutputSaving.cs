namespace SavingService.GraphQL
{
    public record OutputSaving
    {
        public string message { get; set; }
        public double? TotalSaving { get; set; }
        public double? TotalGold { get; set; }
        public DateTime Date { get; set; }
    }
}

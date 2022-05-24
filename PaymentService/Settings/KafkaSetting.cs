namespace PaymentService.Settings
{
    public class KafkaSetting
    {
        public string Server { get; set; }
        public int NumPartitions { get; set; }
        public short ReplicantFactor { get; set; }
    }
}

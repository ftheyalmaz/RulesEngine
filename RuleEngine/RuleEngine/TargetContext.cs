namespace RuleEngine
{
    public class TargetContext
    {
        public string Strategy { get; set; }
        public string SecurityType { get; set; }
        public string ClearingBroker { get; set; }
        public int Sid { get; set; }
        public string Ticker { get; set; }
        public string SecurityDescription { get; set; }
        public double StartPriceN { get; set; }
        public double EndPriceN { get; set; }
        public string EndQuantity { get; set; }
        public double QuantityChange { get; set; }
        public string StartOTE { get; set; }
        public string EndOTE { get; set; }
        public double RealizedPL { get; set; }
        public double TotalPL { get; set; }
        public string EndMarketValue { get; set; }
        public string Manager { get; set; }
        public string LegalEntity { get; set; }
    }
}

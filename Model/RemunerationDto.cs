namespace Model
{
    public class RemunerationDto
    {
        public string cityName { get; set; }
        public UniversalSocialTaxDto universalSocialCharge { get; set; }
        public IncomeTaxDto incomeTax { get; set; }
        public string retirementTax { get; set; }
    }
}

using BusinessLogic.Enum;
using DataAccessLayer;
using Model;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Taxes
{
    public class TaxesManager : ITaxesManager
    {
        #region Propery

        private readonly IDataAccessManager<RemunerationDto> _dataAccess;
        private readonly string _filePath = Constants.FILEPATH;

        #endregion

        #region Constructor

        public TaxesManager(IDataAccessManager<RemunerationDto> dataAccess)
        {
            _dataAccess = dataAccess;
        }

        #endregion

        #region Public Method

        public void InsertTaxesAndRates()
        {
            RemunerationDto remunerationDto = InsertRemunerationParameters();

            _dataAccess.StoreTaxesForCity(_filePath, remunerationDto);
        }

        public void ReadTaxesAndRates()
        {
            IList<RemunerationDto> remunerationDto;
            UserParameter userParameter = new UserParameter();

            Console.WriteLine("Inserisci la Citta");
            userParameter.cityName = Console.ReadLine();

            bool fileExist = _dataAccess.ExistFile(_filePath);

            if (fileExist)
            {
                remunerationDto = _dataAccess.ReadJson(_filePath);
            }
            else
            {
                Console.WriteLine("Non hai città inserite. Si prega di effettuare almeno una registrazione.");
                Console.ReadLine();
                throw new Exception("You don't have any city registgered");
            }

            RemunerationDto remunerationSelectedCity = GetCity(remunerationDto, userParameter.cityName);
            if (remunerationSelectedCity == null)
            {
                Console.WriteLine("Non ho trovato città. Si prega di inserire una città valida");
                Console.ReadLine();
                throw new Exception("You have insered a wrong city. City not found");
            }

            do
            {
                Console.WriteLine("Inserire le ore lavorative con il . come separatore decimale");
                userParameter.workingHour = Console.ReadLine();
            }
            while (CheckForDot(userParameter.workingHour));

            do
            {
                Console.WriteLine("Inserire il costo orario con il . come separatore decimale");
                userParameter.hourlyCost = Console.ReadLine();
            }
            while (CheckForDot(userParameter.hourlyCost));

            TaxesToPrint taxesToPrint = CalculateTaxSalary(remunerationSelectedCity, userParameter);
            Console.WriteLine($"Ammontare lordo: € { taxesToPrint.grossAmount}");
            Console.WriteLine($"Imposta sul reddito: € {taxesToPrint.incomeTax}");
            Console.WriteLine($"Universal Social Rate: € {taxesToPrint.universalSocialRate}");
            Console.WriteLine($"Pensione: € {taxesToPrint.retirementTax}");
            Console.WriteLine($"Ammontare Netto: € {taxesToPrint.netAmount}");
            Console.ReadLine();
        }

        #endregion

        #region Private Method

        private RemunerationDto InsertRemunerationParameters()
        {
            RemunerationDto remunerationDto = new RemunerationDto();
            IncomeTaxDto incomeTaxDto = new IncomeTaxDto();
            Console.WriteLine("Inserisci la Citta");
            remunerationDto.cityName = Console.ReadLine();
            remunerationDto.universalSocialCharge = SetUniversalSocialRate();

            bool choosedIncomeTax;
            do
            {
                choosedIncomeTax = false;
                Console.WriteLine("Dichiarare se l'imposta sul reddito è variabile");
                Console.WriteLine("1 - Imposta sul Reddito Variablie");
                Console.WriteLine("2 - Imposta sul Reddito Fissa");
                incomeTaxDto.incomeTaxIsVariable = Console.ReadLine();
                switch (incomeTaxDto.incomeTaxIsVariable)
                {
                    case "1":
                        incomeTaxDto = InsertIncomeTaxes(incomeTaxDto, IncomeTaxEnum.IsVariable);
                        break;
                    case "2":
                        incomeTaxDto = InsertIncomeTaxes(incomeTaxDto, IncomeTaxEnum.IsFixed);
                        break;
                    default:
                        choosedIncomeTax = true;
                        break;
                }
            }
            while (choosedIncomeTax);

            remunerationDto.incomeTax = incomeTaxDto;

            remunerationDto = InsertRetirementTax(remunerationDto);
            return remunerationDto;
        }

        private UniversalSocialTaxDto SetUniversalSocialRate()
        {
            UniversalSocialTaxDto universalSocialTaxDto = new UniversalSocialTaxDto();
            Console.WriteLine("Inserisci la USR (Universal Social Rate) base");
            Console.WriteLine("Metterla a 0 se il Paese non la prevede");
            universalSocialTaxDto.universalSocialTaxBase = Console.ReadLine();
            if (universalSocialTaxDto.universalSocialTaxBase != "0")
            {
                universalSocialTaxDto.universalSocialTaxHasValue = true;
                do
                {
                    Console.WriteLine("Inserisci la USR (Universal Social Rate) ridotta");
                    universalSocialTaxDto.universalSocialTaxReduced = Console.ReadLine();
                }
                while (CheckForDot(universalSocialTaxDto.universalSocialTaxReduced));

                do
                {
                    Console.WriteLine("Inserisci la soglia per la  USR (Universal Social Rate) piena");
                    universalSocialTaxDto.universalSocialTaxSalaryLevel = Console.ReadLine();
                }
                while (CheckForDot(universalSocialTaxDto.universalSocialTaxSalaryLevel));
            }
            else
            {
                universalSocialTaxDto.universalSocialTaxHasValue = false;
            }

            return universalSocialTaxDto;
        }

        private IncomeTaxDto InsertIncomeTaxes(IncomeTaxDto incomeTaxDto, IncomeTaxEnum incomeTypeTax)
        {
            do
            {
                Console.WriteLine("Inserire la tassa sul reddito con il . come separatore decimale");
                incomeTaxDto.incomeTaxBase = Console.ReadLine();
            }
            while (CheckForDot(incomeTaxDto.incomeTaxBase));
            if (incomeTypeTax == IncomeTaxEnum.IsVariable)
                incomeTaxDto = InsertIncomeVariableTaxes(incomeTaxDto);

            return incomeTaxDto;
        }

        private IncomeTaxDto InsertIncomeVariableTaxes(IncomeTaxDto incomeTaxDto)
        {
            do
            {
                Console.WriteLine("Inserire la tassa sul reddito RIDOTTA con il . come separatore decimale");
                incomeTaxDto.reducedIncomeTax = Console.ReadLine();
            }
            while (CheckForDot(incomeTaxDto.incomeTaxBase));

            Console.WriteLine("Inserire il salario di soglia per la riduzione delle tasse");
            incomeTaxDto.incomeTaxReducedSalaryLevel = Console.ReadLine();

            return incomeTaxDto;
        }

        private RemunerationDto InsertRetirementTax(RemunerationDto remunerationDto)
        {
            do
            {
                Console.WriteLine("Inserire la tassa sulla Pensione con il . come separatore decimale");
                remunerationDto.retirementTax = Console.ReadLine();
            } while (CheckForDot(remunerationDto.retirementTax));

            return remunerationDto;
        }


        private RemunerationDto GetCity(IList<RemunerationDto> remunerationDtoList, string cityName)
        {
            RemunerationDto remunerationDto = new RemunerationDto();

            foreach (var remunerationCity in remunerationDtoList)
            {
                if (cityName.ToLower() == remunerationCity.cityName.ToLower())
                {
                    remunerationDto = remunerationCity;
                }
            }

            return remunerationDto;
        }

        private TaxesToPrint CalculateTaxSalary(RemunerationDto remunerationSelectedCity, UserParameter userParameter)
        {
            TaxesToPrint taxesToPrint = new TaxesToPrint();

            taxesToPrint.grossAmount = CalculateGrossAmount(userParameter);

            taxesToPrint.incomeTax = CalculateIncomeTax(remunerationSelectedCity, taxesToPrint.grossAmount);

            taxesToPrint.retirementTax = CalculateRetirementTax(remunerationSelectedCity, taxesToPrint.grossAmount);

            taxesToPrint.universalSocialRate = CalculateUniversalSocialRate(remunerationSelectedCity, taxesToPrint.grossAmount);

            taxesToPrint.netAmount = taxesToPrint.grossAmount - taxesToPrint.incomeTax - taxesToPrint.retirementTax - taxesToPrint.universalSocialRate;

            return taxesToPrint;
        }

        private double CalculateUniversalSocialRate(RemunerationDto remunerationSelectedCity, double grossAmount)
        {
            double universalSocialRate = 0;
            bool universalSocialRateHasValue;
            double universalSocialTaxBase;
            double universalSocialTaxReduced;
            double universalSocialTaxSalaryLevel;

            try
            {
                universalSocialRateHasValue = remunerationSelectedCity.universalSocialCharge.universalSocialTaxHasValue;
                if (universalSocialRateHasValue)
                {
                    universalSocialTaxSalaryLevel = double.Parse(remunerationSelectedCity.universalSocialCharge.universalSocialTaxSalaryLevel);
                    if (grossAmount <= universalSocialTaxSalaryLevel)
                    {
                        universalSocialTaxReduced = double.Parse(remunerationSelectedCity.universalSocialCharge.universalSocialTaxReduced);
                        universalSocialRate = (grossAmount * universalSocialTaxReduced) / Constants.PERCENTAGE;
                    }
                    else
                    {
                        universalSocialTaxBase = double.Parse(remunerationSelectedCity.universalSocialCharge.universalSocialTaxBase);
                        universalSocialRate = (grossAmount * universalSocialTaxBase) / Constants.PERCENTAGE;
                    }
                }
                else
                    universalSocialRate = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion string to Number Failed in CalculateUniversalSocialRate");
                Console.ReadLine();
                throw new InvalidCastException("Exception in CalculateUniversalSocialRate", ex.InnerException);
            }

            return universalSocialRate;
        }

        private double CalculateRetirementTax(RemunerationDto remunerationSelectedCity, double grossAmount)
        {
            double retirementTax = 0;

            try
            {
                retirementTax = double.Parse(remunerationSelectedCity.retirementTax);

                retirementTax = (grossAmount * retirementTax) / Constants.PERCENTAGE;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion string to Number Failed in CalculateRetirementTax.");
                Console.ReadLine();
                throw new InvalidCastException("Exception in CalculateRetirementTax", ex.InnerException);
            }

            return retirementTax;
        }

        private double CalculateIncomeTax(RemunerationDto remunerationSelectedCity, double grossAmount)
        {
            bool taxVariable = false;
            double incomeTaxes = 0;
            double incomeTaxReducedSalaryLevel = 0;
            double reducedIncomeTax = 0;
            double taxedAmountOverTheMinimum = 0;
            double incomeBaseTax = double.Parse(remunerationSelectedCity.incomeTax.incomeTaxBase);

            try
            {
                if (remunerationSelectedCity.incomeTax.incomeTaxIsVariable == "1")
                    taxVariable = true;

                if (taxVariable)
                {
                    incomeTaxReducedSalaryLevel = double.Parse(remunerationSelectedCity.incomeTax.incomeTaxReducedSalaryLevel);
                    reducedIncomeTax = double.Parse(remunerationSelectedCity.incomeTax.reducedIncomeTax);
                }

                if (taxVariable && (grossAmount <= incomeTaxReducedSalaryLevel))
                        incomeTaxes = (grossAmount * reducedIncomeTax) / Constants.PERCENTAGE;
                else if (taxVariable && (grossAmount > incomeTaxReducedSalaryLevel))
                {
                    taxedAmountOverTheMinimum = grossAmount - incomeTaxReducedSalaryLevel;
                    incomeTaxes = (incomeTaxReducedSalaryLevel * reducedIncomeTax) / Constants.PERCENTAGE;
                    incomeTaxes = incomeTaxes + (taxedAmountOverTheMinimum * incomeBaseTax / Constants.PERCENTAGE);
                }
                else if (!taxVariable || (grossAmount > incomeTaxReducedSalaryLevel))
                {
                    incomeTaxes = (grossAmount * incomeBaseTax) / Constants.PERCENTAGE;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion string to Number Failed in CalculateIncomeTax.");
                Console.ReadLine();
                throw new InvalidCastException("Exception in CalculateIncomeTax", ex.InnerException);
            }

            return incomeTaxes;
        }

        private double CalculateGrossAmount(UserParameter userParameter)
        {
            int workingHour = 0;
            double hourlyCost = 0;

            try
            {
                workingHour = int.Parse(userParameter.workingHour);
                hourlyCost = double.Parse(userParameter.hourlyCost);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion string to Number Failed in Calculate Gross Amout.");
                Console.ReadLine();
                throw new InvalidCastException("Exception in CalculateGrossAmount", ex.InnerException);
            }

            double grossAmount = hourlyCost * workingHour;

            return grossAmount;
        }

        private bool CheckForDot(string fieldToCheck)
        {
            bool checkForDot = false;

            if (fieldToCheck.ToLower().Contains(","))
                checkForDot = true;

            return checkForDot;
        }

        #endregion
    }
}

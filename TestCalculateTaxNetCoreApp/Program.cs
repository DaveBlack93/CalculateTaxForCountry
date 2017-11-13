using System;
using Microsoft.Extensions.DependencyInjection;
using BusinessLogic.Taxes;
using DataAccessLayer;
using Model;

namespace TestCalculateTaxNetCoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ITaxesManager,TaxesManager>()
                .AddSingleton<IDataAccessManager<RemunerationDto>, DataAccessManager<RemunerationDto>>()
                .BuildServiceProvider();
            var _manageTaxes = serviceProvider.GetService<ITaxesManager>();

            string selection = "";
            Console.WriteLine("Digita il numero corrispondente alla tua scelta");
            Console.WriteLine("1 - Inserimento nuovo Stato");
            Console.WriteLine("2 - Calcola stipendio netto");
            selection = Console.ReadLine();
            switch (selection)
            {
                case "1":
                    _manageTaxes.InsertTaxesAndRates();
                    break;
                case "2":
                    _manageTaxes.ReadTaxesAndRates();
                    break;
                default:
                    break;
            }

            Console.ReadLine();
        }
    }
}

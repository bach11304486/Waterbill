using System;
using System.Collections.Generic;
using System.Linq;

namespace SE08204_ElectricBill
{
    public class Customer
    {
        // Fields
        private string fullName;
        private string id;
        private string typeCustomer;
        private int lastMonthAmount;
        private int thisMonthAmount;
        private int amountPaid;
        private double vatFee;
        private double environmentalFee;
        internal object Consumption;

        public double WaterUsage { get; set; } // m³
        public double PricePerM3 { get; set; }

        public double GetWaterFee()
        {
            return WaterUsage * PricePerM3;
        }

        public double GetEnvironmentalFee()
        {
            return GetWaterFee() * 0.1; // 10% phí môi trường
        }

        public double GetVAT()
        {
            return (GetWaterFee() + GetEnvironmentalFee()) * 0.1; // 10% VAT
        }

        public double GetTotal()
        {
            return GetWaterFee() + GetEnvironmentalFee() + GetVAT();
        }

        // Properties
        public string FullName => fullName;
        public string Id => id;
        public string TypeCustomer => typeCustomer;
        public int LastMonthAmount => lastMonthAmount;
        public int ThisMonthAmount => thisMonthAmount;
        public int AmountPaid => amountPaid;
        public double VatFee => vatFee;
        public double EnvironmentalFee => environmentalFee;

        // Constructor
        public Customer(string fullName, string id, string typeCustomer, int lastMonthAmount, int thisMonthAmount)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be empty.", nameof(id));
            if (!new[] { "Household", "Business", "Administration", "Factory" }.Contains(typeCustomer))
                throw new ArgumentException("Invalid customer type.", nameof(typeCustomer));

            this.fullName = fullName;
            this.id = id;
            this.typeCustomer = typeCustomer;
            this.lastMonthAmount = lastMonthAmount;
            this.thisMonthAmount = thisMonthAmount;

            int baseBill = CalculateElectricityBill();
            this.environmentalFee = baseBill * 0.05; // 5%
            this.vatFee = (baseBill + environmentalFee) * 0.10; // 10% VAT
            this.amountPaid = baseBill + (int)environmentalFee + (int)vatFee;
        }

        // Calculate base electricity bill
        public int CalculateElectricityBill()
        {
            int consumed = thisMonthAmount - lastMonthAmount;
            if (consumed < 0) return 0;

            var pricingTiers = new Dictionary<string, (int threshold, int price)[]>
            {
                { "Household", new[] { (100, 2000), (200, 2500), (int.MaxValue, 3000) } },
                { "Business", new[] { (200, 3000), (500, 3500), (int.MaxValue, 4000) } },
                { "Administration", new[] { (150, 2500), (300, 3000), (int.MaxValue, 3500) } },
                { "Factory", new[] { (500, 3500), (1000, 4000), (int.MaxValue, 4500) } }
            };

            var tiers = pricingTiers[typeCustomer];
            int totalCost = 0;
            int remainingConsumed = consumed;
            int previousThreshold = 0;

            foreach (var (threshold, price) in tiers)
            {
                int tierConsumption = Math.Min(remainingConsumed, threshold - previousThreshold);
                if (tierConsumption > 0)
                {
                    totalCost += tierConsumption * price;
                    remainingConsumed -= tierConsumption;
                }
                previousThreshold = threshold;
                if (remainingConsumed <= 0) break;
            }

            return totalCost;
        }
    }
}

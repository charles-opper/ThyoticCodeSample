#region Instructions
/*
 * You are tasked with writing an algorithm that determines the value of a used car, 
 * given several factors.
 * 
 *    AGE:    Given the number of months of how old the car is, reduce its value one-half 
 *            (0.5) percent.
 *            After 10 years, it's value cannot be reduced further by age. This is not 
 *            cumulative.
 *            
 *    MILES:    For every 1,000 miles on the car, reduce its value by one-fifth of a
 *              percent (0.2). Do not consider remaining miles. After 150,000 miles, it's 
 *              value cannot be reduced further by miles.
 *            
 *    PREVIOUS OWNER:    If the car has had more than 2 previous owners, reduce its value 
 *                       by twenty-five (25) percent. If the car has had no previous  
 *                       owners, add ten (10) percent of the FINAL car value at the end.
 *                    
 *    COLLISION:        For every reported collision the car has been in, remove two (2) 
 *                      percent of it's value up to five (5) collisions.
 *                    
 * 
 *    Each factor should be off of the result of the previous value in the order of
 *        1. AGE
 *        2. MILES
 *        3. PREVIOUS OWNER
 *        4. COLLISION
 *        
 *    E.g., Start with the current value of the car, then adjust for age, take that  
 *    result then adjust for miles, then collision, and finally previous owner. 
 *    Note that if previous owner, had a positive effect, then it should be applied 
 *    AFTER step 4. If a negative effect, then BEFORE step 4.
 */
#endregion

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace CarPricer
{
    public class Car
    {
        public decimal PurchaseValue { get; set; }
        public int AgeInMonths { get; set; }
        public int NumberOfMiles { get; set; }
        public int NumberOfPreviousOwners { get; set; }
        public int NumberOfCollisions { get; set; }
    }

    public class PriceDeterminator
    {
        // For this example, using constants. A real life application would load these from a lookup table, database or config file
        const decimal AgeReductionFactor = 0.005M;
        const int MaxReductionYearsInMonths = 120;

        const int MileageReductionStep = 1000;
        const decimal MileageReductionFactor = 0.002M;
        const int MaxMilesForReduction = 150000;

        const int PreviousOwnerReductionNumber = 2;
        const decimal PreviousOwnerReductionFactor = 0.25M;
        const decimal NoPreviousOwnerFactor = 0.10M;

        const decimal CollisionReductionFactor = 0.02M;
        const int MaxCollisionsForReduciton = 5;

        public decimal DetermineCarPrice(Car car)
        {
            // Assume we are given a valid car reference. If not, fail fast.
            Debug.Assert(car != null);

            // Car price starts at the purchase value
            decimal carPrice = car.PurchaseValue;

            // Reduce car price based on cumulative factors
            carPrice = ReduceValueOnAge(carPrice, car.AgeInMonths);
            carPrice = ReduceValueOnMiles(carPrice, car.NumberOfMiles);

            var addNoPreviousOwnerBonus = car.NumberOfPreviousOwners == 0;
            if (!addNoPreviousOwnerBonus)
            {
                // If no previous owner bonus, then apply normal previous owner reduction
                carPrice = ReduceValueOnPreviousOwners(carPrice, car.NumberOfPreviousOwners);
            }

            carPrice = ReduceValueOnCollisions(carPrice, car.NumberOfCollisions);

            if (addNoPreviousOwnerBonus)
            {
                carPrice = AddPreviousOwnerBonus(carPrice);   
            }

            return carPrice;
        }

        private decimal ReduceValueOnAge(decimal carPrice, int ageInMonths)
        {
            if (ageInMonths <= MaxReductionYearsInMonths)
            {
                carPrice = carPrice - (ageInMonths * (carPrice * AgeReductionFactor));
            }
            else
            {
                carPrice = carPrice - (MaxReductionYearsInMonths * (carPrice * AgeReductionFactor));
            }

            return carPrice;
        }

        private decimal ReduceValueOnMiles(decimal carPrice, int miles)
        {
            if (miles <= MaxMilesForReduction)
            {
                carPrice = carPrice - ((miles / MileageReductionStep) * (carPrice * MileageReductionFactor));
            }
            else
            {
                carPrice = carPrice - ((MaxMilesForReduction / MileageReductionStep) * (carPrice * MileageReductionFactor));
            }

            return carPrice;
        }

        private decimal ReduceValueOnPreviousOwners(decimal carPrice, int numPrevOwners)
        {
            if (numPrevOwners > PreviousOwnerReductionNumber)
            {
                carPrice = carPrice - carPrice * PreviousOwnerReductionFactor;
            }

            return carPrice;
        }        
        
        private decimal ReduceValueOnCollisions(decimal carPrice, int numCollisions)
        {
            if (numCollisions < MaxCollisionsForReduciton)
            {
                carPrice = carPrice - numCollisions * carPrice * CollisionReductionFactor;
            }

            return carPrice;
        }

        private decimal AddPreviousOwnerBonus(decimal carPrice)
        {
            return carPrice + carPrice * NoPreviousOwnerFactor;
        }

    }


    [TestFixture]
    public class UnitTests
    {

        [Test]
        public void CalculateCarValue()
        {
            AssertCarValue(25313.40m, 35000m, 3 * 12, 50000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 150000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 250000, 1, 1);
            AssertCarValue(20090.00m, 35000m, 3 * 12, 250000, 1, 0);
            AssertCarValue(21657.02m, 35000m, 3 * 12, 250000, 0, 1);
        }

        private static void AssertCarValue(decimal expectValue, decimal purchaseValue,
        int ageInMonths, int numberOfMiles, int numberOfPreviousOwners, int
        numberOfCollisions)
        {
            Car car = new Car
            {
                AgeInMonths = ageInMonths,
                NumberOfCollisions = numberOfCollisions,
                NumberOfMiles = numberOfMiles,
                NumberOfPreviousOwners = numberOfPreviousOwners,
                PurchaseValue = purchaseValue
            };
            PriceDeterminator priceDeterminator = new PriceDeterminator();
            var carPrice = priceDeterminator.DetermineCarPrice(car);
            Assert.AreEqual(expectValue, carPrice);
        }
    }
}
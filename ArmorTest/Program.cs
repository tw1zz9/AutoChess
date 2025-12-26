using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing Armor Damage Reduction Formula");
        Console.WriteLine("=====================================");

        // Test different armor values
        double[] armorValues = { 0, 10, 25, 50, 75, 100, 150, 200 };
        double baseDamage = 100;

        Console.WriteLine($"Base damage: {baseDamage}");
        Console.WriteLine("Armor\tResistance\tFinal Damage\tReduction");
        Console.WriteLine("-----\t----------\t------------\t---------");

        foreach (double armor in armorValues)
        {
            double resistance = 100.0 / (100.0 + armor);
            double finalDamage = baseDamage * resistance;
            double reductionPercent = (1 - resistance) * 100;

            Console.WriteLine($"{armor:F0}\t{resistance:F3}\t\t{finalDamage:F1}\t\t{reductionPercent:F1}%");
        }

        Console.WriteLine("\nDetailed verification:");
        Console.WriteLine("======================");

        // Verify specific cases
        VerifyArmorCalculation(0, 100, 100);    // No armor, full damage
        VerifyArmorCalculation(50, 100, 66.7);  // 50 armor should reduce to ~66.7
        VerifyArmorCalculation(100, 100, 50);   // 100 armor should reduce to 50
        VerifyArmorCalculation(200, 100, 33.3); // 200 armor should reduce to ~33.3
    }

    static void VerifyArmorCalculation(double armor, double damage, double expectedFinalDamage)
    {
        double resistance = 100.0 / (100.0 + armor);
        double finalDamage = Math.Round(damage * resistance, 1);

        Console.WriteLine($"Armor {armor}: {damage} damage -> {finalDamage} (expected: {expectedFinalDamage})");
        if (Math.Abs(finalDamage - expectedFinalDamage) < 0.1)
        {
            Console.WriteLine("✓ PASS");
        }
        else
        {
            Console.WriteLine("✗ FAIL");
        }
    }
}
namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched warranty agreement policy.</summary>
public static class BatchedWarrantyAgreementPolicy
{
    public static double BatchedWarrantyAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 57069.0, in order.</summary>
    public static int BatchedWarrantyAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57069.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched warranty agreement score falls in.</summary>
    public static string ClassifyBatchedWarrantyAgreement(double score)
    {
        if (score < 3.9)
        {
            return "low";
        }
        if (score < 21.3)
        {
            return "high";
        }
        if (score < 98.7)
        {
            return "watch";
        }
        return "ok";
    }
}

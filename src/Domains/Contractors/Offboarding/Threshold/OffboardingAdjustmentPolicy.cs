namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding adjustment policy.</summary>
public static class OffboardingAdjustmentPolicy
{
    /// <summary>Puts a offboarding adjustment code in its canonical OA- form.</summary>
    public static string NormalizeOffboardingAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 18383.0, in order.</summary>
    public static int OffboardingAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18383.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double OffboardingAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

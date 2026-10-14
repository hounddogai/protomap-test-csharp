namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction agreement policy.</summary>
public static class DeductionAgreementPolicy
{
    public static double DeductionAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a deduction agreement code in its canonical DA- form.</summary>
    public static string NormalizeDeductionAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a deduction agreement score falls in.</summary>
    public static string ClassifyDeductionAgreement(double score)
    {
        if (score < 30.0)
        {
            return "elevated";
        }
        if (score < 57.1)
        {
            return "ok";
        }
        if (score < 68.4)
        {
            return "medium";
        }
        if (score < 96.2)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 17483.0, in order.</summary>
    public static int DeductionAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17483.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

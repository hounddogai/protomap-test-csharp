namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy estimate policy.</summary>
public static class PolicyEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74188.0, in order.</summary>
    public static int PolicyEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74188.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PolicyEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a policy estimate code in its canonical PE- form.</summary>
    public static string NormalizePolicyEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan manifest policy.</summary>
public static class ReturnPlanManifestPolicy
{
    public static string ReturnPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 82700.0, in order.</summary>
    public static int ReturnPlanManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82700.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a return plan manifest code in its canonical RPM- form.</summary>
    public static string NormalizeReturnPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

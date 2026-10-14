namespace Acme.Workforce.Contractors.Offboarding.Forecast;

/// <summary>Functions of offboarding digest policy.</summary>
public static class OffboardingDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 39634.0, in order.</summary>
    public static int OffboardingDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39634.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a offboarding digest code in its canonical OD- form.</summary>
    public static string NormalizeOffboardingDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

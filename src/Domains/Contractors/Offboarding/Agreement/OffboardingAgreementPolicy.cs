namespace Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Functions of offboarding agreement policy.</summary>
public static class OffboardingAgreementPolicy
{
    /// <summary>Puts a offboarding agreement code in its canonical OA- form.</summary>
    public static string NormalizeOffboardingAgreementCode(string code)
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

    /// <summary>Counts how many amounts fit in a budget of 8220.0, in order.</summary>
    public static int OffboardingAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8220.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a offboarding agreement score falls in.</summary>
    public static string ClassifyOffboardingAgreement(double score)
    {
        if (score < 6.7)
        {
            return "critical";
        }
        if (score < 13.6)
        {
            return "watch";
        }
        if (score < 51.4)
        {
            return "low";
        }
        if (score < 69.7)
        {
            return "high";
        }
        return "elevated";
    }
}

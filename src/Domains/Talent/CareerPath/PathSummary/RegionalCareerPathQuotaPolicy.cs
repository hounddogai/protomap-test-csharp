namespace Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Functions of regional career path quota policy.</summary>
public static class RegionalCareerPathQuotaPolicy
{
    /// <summary>Puts a regional career path quota code in its canonical RCPQ- form.</summary>
    public static string NormalizeRegionalCareerPathQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RCPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RCPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 3779.0, in order.</summary>
    public static int RegionalCareerPathQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3779.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Functions of career path manifest policy.</summary>
public static class CareerPathManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36793.0, in order.</summary>
    public static int CareerPathManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36793.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a career path manifest score falls in.</summary>
    public static string ClassifyCareerPathManifest(double score)
    {
        if (score < 4.3)
        {
            return "elevated";
        }
        if (score < 4.9)
        {
            return "critical";
        }
        if (score < 59.2)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Puts a career path manifest code in its canonical CPM- form.</summary>
    public static string NormalizeCareerPathManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

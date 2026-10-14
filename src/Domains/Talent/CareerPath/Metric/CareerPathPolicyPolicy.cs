namespace Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Functions of career path policy policy.</summary>
public static class CareerPathPolicyPolicy
{
    /// <summary>Names the band a career path policy score falls in.</summary>
    public static string ClassifyCareerPathPolicy(double score)
    {
        if (score < 9.1)
        {
            return "critical";
        }
        if (score < 36.8)
        {
            return "ok";
        }
        if (score < 47.2)
        {
            return "elevated";
        }
        if (score < 79.8)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 59148.0, in order.</summary>
    public static int CareerPathPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59148.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CareerPathPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a career path policy code in its canonical CPP- form.</summary>
    public static string NormalizeCareerPathPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

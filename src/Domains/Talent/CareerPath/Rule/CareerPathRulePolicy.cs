namespace Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Functions of career path rule policy.</summary>
public static class CareerPathRulePolicy
{
    public static int CareerPathRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a career path rule code in its canonical CPR- form.</summary>
    public static string NormalizeCareerPathRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a career path rule score falls in.</summary>
    public static string ClassifyCareerPathRule(double score)
    {
        if (score < 16.0)
        {
            return "ok";
        }
        if (score < 61.0)
        {
            return "minimal";
        }
        return "medium";
    }
}

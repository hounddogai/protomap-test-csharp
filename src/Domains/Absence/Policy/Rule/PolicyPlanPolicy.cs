namespace Acme.Workforce.Absence.Policy.Rule;

/// <summary>Functions of policy plan policy.</summary>
public static class PolicyPlanPolicy
{
    public static int PolicyPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a policy plan score falls in.</summary>
    public static string ClassifyPolicyPlan(double score)
    {
        if (score < 40.1)
        {
            return "medium";
        }
        if (score < 45.4)
        {
            return "critical";
        }
        if (score < 80.9)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Puts a policy plan code in its canonical PP- form.</summary>
    public static string NormalizePolicyPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Functions of return plan target policy.</summary>
public static class ReturnPlanTargetPolicy
{
    /// <summary>Puts a return plan target code in its canonical RPT- form.</summary>
    public static string NormalizeReturnPlanTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a return plan target score falls in.</summary>
    public static string ClassifyReturnPlanTarget(double score)
    {
        if (score < 52.4)
        {
            return "medium";
        }
        if (score < 57.0)
        {
            return "elevated";
        }
        return "ok";
    }
}

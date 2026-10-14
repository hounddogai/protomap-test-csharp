namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan review policy.</summary>
public static class ReturnPlanReviewPolicy
{
    public static string ReturnPlanReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a return plan review code in its canonical RPR- form.</summary>
    public static string NormalizeReturnPlanReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

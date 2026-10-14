namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan notice policy.</summary>
public static class ReturnPlanNoticePolicy
{
    /// <summary>Puts a return plan notice code in its canonical RPN- form.</summary>
    public static string NormalizeReturnPlanNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ReturnPlanNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary succession plan claim policy.</summary>
public static class PrimarySuccessionPlanClaimPolicy
{
    public static double PrimarySuccessionPlanClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary succession plan claim code in its canonical PSPC- form.</summary>
    public static string NormalizePrimarySuccessionPlanClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimarySuccessionPlanClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}

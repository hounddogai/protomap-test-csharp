namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient action plan baseline policy.</summary>
public static class LenientActionPlanBaselinePolicy
{
    /// <summary>Names the band a lenient action plan baseline score falls in.</summary>
    public static string ClassifyLenientActionPlanBaseline(double score)
    {
        if (score < 33.5)
        {
            return "high";
        }
        if (score < 56.3)
        {
            return "elevated";
        }
        if (score < 81.8)
        {
            return "watch";
        }
        if (score < 92.9)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Puts a lenient action plan baseline code in its canonical LAPB- form.</summary>
    public static string NormalizeLenientActionPlanBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LAPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LAPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientActionPlanBaseline()
    {
        var policy = Retry.DefaultPolicy("lenient-action-plan-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double LenientActionPlanBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

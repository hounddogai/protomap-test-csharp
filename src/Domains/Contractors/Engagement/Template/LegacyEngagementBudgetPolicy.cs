namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy engagement budget policy.</summary>
public static class LegacyEngagementBudgetPolicy
{
    public static double LegacyEngagementBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyEngagementBudget()
    {
        var policy = Retry.DefaultPolicy("legacy-engagement-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a legacy engagement budget code in its canonical LEB- form.</summary>
    public static string NormalizeLegacyEngagementBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LEB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LEB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global succession plan request policy.</summary>
public static class GlobalSuccessionPlanRequestPolicy
{
    public static List<string> MergeGlobalSuccessionPlanRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalSuccessionPlanRequest()
    {
        var policy = Retry.DefaultPolicy("global-succession-plan-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a global succession plan request code in its canonical GSPR- form.</summary>
    public static string NormalizeGlobalSuccessionPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 52545.0, in order.</summary>
    public static int GlobalSuccessionPlanRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52545.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

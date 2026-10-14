namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered return plan rule policy.</summary>
public static class TieredReturnPlanRulePolicy
{
    /// <summary>Puts a tiered return plan rule code in its canonical TRPR- form.</summary>
    public static string NormalizeTieredReturnPlanRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TRPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TRPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TieredReturnPlanRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredReturnPlanRule()
    {
        var policy = Retry.DefaultPolicy("tiered-return-plan-rule");
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

    /// <summary>Counts how many amounts fit in a budget of 40221.0, in order.</summary>
    public static int TieredReturnPlanRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40221.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

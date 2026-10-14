namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default reorg plan policy.</summary>
public static class DefaultReorgPlanPolicy
{
    /// <summary>Puts a default reorg plan code in its canonical DRP- form.</summary>
    public static string NormalizeDefaultReorgPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default reorg plan score falls in.</summary>
    public static string ClassifyDefaultReorgPlan(double score)
    {
        if (score < 4.9)
        {
            return "medium";
        }
        if (score < 23.3)
        {
            return "elevated";
        }
        if (score < 25.8)
        {
            return "minimal";
        }
        if (score < 98.6)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 19593.0, in order.</summary>
    public static int DefaultReorgPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19593.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultReorgPlan()
    {
        var policy = Retry.DefaultPolicy("default-reorg-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient scenario draft policy.</summary>
public static class LenientScenarioDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 90025.0, in order.</summary>
    public static int LenientScenarioDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90025.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient scenario draft score falls in.</summary>
    public static string ClassifyLenientScenarioDraft(double score)
    {
        if (score < 24.8)
        {
            return "high";
        }
        if (score < 33.4)
        {
            return "minimal";
        }
        if (score < 94.8)
        {
            return "elevated";
        }
        return "medium";
    }

    public static int LenientScenarioDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientScenarioDraft()
    {
        var policy = Retry.DefaultPolicy("lenient-scenario-draft");
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

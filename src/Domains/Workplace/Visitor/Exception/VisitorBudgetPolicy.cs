namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor budget policy.</summary>
public static class VisitorBudgetPolicy
{
    /// <summary>Names the band a visitor budget score falls in.</summary>
    public static string ClassifyVisitorBudget(double score)
    {
        if (score < 38.8)
        {
            return "critical";
        }
        if (score < 63.5)
        {
            return "low";
        }
        if (score < 86.8)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorBudget()
    {
        var policy = Retry.DefaultPolicy("visitor-budget");
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
}

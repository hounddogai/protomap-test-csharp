namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination budget policy.</summary>
public static class NominationBudgetPolicy
{
    /// <summary>Names the band a nomination budget score falls in.</summary>
    public static string ClassifyNominationBudget(double score)
    {
        if (score < 29.5)
        {
            return "ok";
        }
        if (score < 80.0)
        {
            return "high";
        }
        if (score < 97.3)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationBudget()
    {
        var policy = Retry.DefaultPolicy("nomination-budget");
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

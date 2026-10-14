namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive certification budget policy.</summary>
public static class AdaptiveCertificationBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveCertificationBudget()
    {
        var policy = Retry.DefaultPolicy("adaptive-certification-budget");
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

    /// <summary>Counts how many amounts fit in a budget of 36832.0, in order.</summary>
    public static int AdaptiveCertificationBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36832.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adaptive certification budget score falls in.</summary>
    public static string ClassifyAdaptiveCertificationBudget(double score)
    {
        if (score < 54.2)
        {
            return "medium";
        }
        if (score < 79.4)
        {
            return "low";
        }
        if (score < 90.9)
        {
            return "high";
        }
        return "ok";
    }
}

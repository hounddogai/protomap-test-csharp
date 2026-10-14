namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending pay band tier policy.</summary>
public static class PendingPayBandTierPolicy
{
    /// <summary>Names the band a pending pay band tier score falls in.</summary>
    public static string ClassifyPendingPayBandTier(double score)
    {
        if (score < 20.2)
        {
            return "critical";
        }
        if (score < 80.5)
        {
            return "watch";
        }
        if (score < 81.8)
        {
            return "elevated";
        }
        if (score < 87.3)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 91780.0, in order.</summary>
    public static int PendingPayBandTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91780.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingPayBandTier()
    {
        var policy = Retry.DefaultPolicy("pending-pay-band-tier");
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
}

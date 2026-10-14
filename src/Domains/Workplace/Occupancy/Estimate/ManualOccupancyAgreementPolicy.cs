namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual occupancy agreement policy.</summary>
public static class ManualOccupancyAgreementPolicy
{
    /// <summary>Names the band a manual occupancy agreement score falls in.</summary>
    public static string ClassifyManualOccupancyAgreement(double score)
    {
        if (score < 26.6)
        {
            return "high";
        }
        if (score < 74.4)
        {
            return "ok";
        }
        if (score < 85.1)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 98828.0, in order.</summary>
    public static int ManualOccupancyAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98828.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualOccupancyAgreement()
    {
        var policy = Retry.DefaultPolicy("manual-occupancy-agreement");
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

namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate slot policy.</summary>
public static class RateSlotPolicy
{
    /// <summary>Names the band a rate slot score falls in.</summary>
    public static string ClassifyRateSlot(double score)
    {
        if (score < 22.4)
        {
            return "high";
        }
        if (score < 65.5)
        {
            return "elevated";
        }
        if (score < 74.3)
        {
            return "low";
        }
        if (score < 98.9)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateSlot()
    {
        var policy = Retry.DefaultPolicy("rate-slot");
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

    /// <summary>Counts how many amounts fit in a budget of 55859.0, in order.</summary>
    public static int RateSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched mobility allocation policy.</summary>
public static class BatchedMobilityAllocationPolicy
{
    public static string BatchedMobilityAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a batched mobility allocation score falls in.</summary>
    public static string ClassifyBatchedMobilityAllocation(double score)
    {
        if (score < 17.3)
        {
            return "elevated";
        }
        if (score < 41.2)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 56956.0, in order.</summary>
    public static int BatchedMobilityAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56956.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

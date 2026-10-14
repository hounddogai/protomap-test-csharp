namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched license seat profile policy.</summary>
public static class BatchedLicenseSeatProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 19980.0, in order.</summary>
    public static int BatchedLicenseSeatProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19980.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string BatchedLicenseSeatProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a batched license seat profile score falls in.</summary>
    public static string ClassifyBatchedLicenseSeatProfile(double score)
    {
        if (score < 9.8)
        {
            return "high";
        }
        if (score < 34.4)
        {
            return "critical";
        }
        if (score < 99.1)
        {
            return "medium";
        }
        return "low";
    }
}

namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted nomination variance policy.</summary>
public static class WeightedNominationVariancePolicy
{
    /// <summary>Names the band a weighted nomination variance score falls in.</summary>
    public static string ClassifyWeightedNominationVariance(double score)
    {
        if (score < 52.2)
        {
            return "medium";
        }
        if (score < 95.5)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 28452.0, in order.</summary>
    public static int WeightedNominationVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28452.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedNominationVarianceSpan(int start, int end)
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
        return span / 86400;
    }

    public static string WeightedNominationVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

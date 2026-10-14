namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination target policy.</summary>
public static class NominationTargetPolicy
{
    public static string NominationTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a nomination target score falls in.</summary>
    public static string ClassifyNominationTarget(double score)
    {
        if (score < 56.3)
        {
            return "elevated";
        }
        if (score < 85.1)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 89456.0, in order.</summary>
    public static int NominationTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89456.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Functions of scheduled nomination exception from digest.</summary>
public static class ScheduledNominationExceptionFromDigest
{
    public static Dictionary<string, double> SummarizeTieredSkillProfileDigestsForException(List<TieredSkillProfileDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<ScheduledNominationException> DeriveScheduledNominationExceptionsFromDigest(List<TieredSkillProfileDigest> items)
    {
        var outValue = new List<ScheduledNominationException>();
        foreach (var item in items)
        {
            var derived = ScheduledNominationException.NewScheduledNominationException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceScheduledNominationException(ScheduledNominationException item)
    {
        return SkillProfileReviewPolicy.SkillProfileReviewWeight(3, true);
    }
}

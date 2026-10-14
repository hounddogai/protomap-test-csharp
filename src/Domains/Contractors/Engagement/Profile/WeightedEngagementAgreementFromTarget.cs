namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Functions of weighted engagement agreement from target.</summary>
public static class WeightedEngagementAgreementFromTarget
{
    public static Dictionary<string, double> SummarizeStatementOfWorkTargetsForAgreement(List<StatementOfWorkTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<WeightedEngagementAgreement> DeriveWeightedEngagementAgreementsFromTarget(List<StatementOfWorkTarget> items)
    {
        var outValue = new List<WeightedEngagementAgreement>();
        foreach (var item in items)
        {
            var derived = WeightedEngagementAgreement.NewWeightedEngagementAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankWeightedEngagementAgreement(WeightedEngagementAgreement item)
    {
        return PendingStatementOfWorkPreferencePolicy.PendingStatementOfWorkPreferenceWeight(3, true);
    }
}

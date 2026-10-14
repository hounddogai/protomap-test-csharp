namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Functions of statement of work override from proposal.</summary>
public static class StatementOfWorkOverrideFromProposal
{
    public static Dictionary<string, double> SummarizeStrictRateProposalsForOverride(List<StrictRateProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StatementOfWorkOverride> DeriveStatementOfWorkOverridesFromProposal(List<StrictRateProposal> items)
    {
        var outValue = new List<StatementOfWorkOverride>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkOverride.NewStatementOfWorkOverride(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStatementOfWorkOverride(StatementOfWorkOverride item)
    {
        return RateProfilePolicy.RateProfileBucket(item.ProjectedVolume());
    }
}

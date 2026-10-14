namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Functions of desk preference from metric.</summary>
public static class DeskPreferenceFromMetric
{
    public static Dictionary<string, double> SummarizeDeskMetricsForPreference(List<DeskMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DeskPreference> DeriveDeskPreferencesFromMetric(List<DeskMetric> items)
    {
        var outValue = new List<DeskPreference>();
        foreach (var item in items)
        {
            var derived = DeskPreference.NewDeskPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateDeskPreference(DeskPreference item)
    {
        return DeskProposalPolicy.NormalizeDeskProposalCode(item.Code);
    }
}

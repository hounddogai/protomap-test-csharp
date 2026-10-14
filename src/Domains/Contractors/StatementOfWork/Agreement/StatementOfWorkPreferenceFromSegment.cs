namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>Functions of statement of work preference from segment.</summary>
public static class StatementOfWorkPreferenceFromSegment
{
    public static Dictionary<string, double> SummarizeStatementOfWorkSegmentsForPreference(List<StatementOfWorkSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<StatementOfWorkPreference> DeriveStatementOfWorkPreferencesFromSegment(List<StatementOfWorkSegment> items)
    {
        var outValue = new List<StatementOfWorkPreference>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkPreference.NewStatementOfWorkPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchStatementOfWorkPreference(StatementOfWorkPreference item)
    {
        return StatementOfWorkSegmentPolicy.ClassifyStatementOfWorkSegment(item.WeightedVolume());
    }
}

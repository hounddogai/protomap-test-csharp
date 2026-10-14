namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Functions of capped timesheet group from template.</summary>
public static class CappedTimesheetGroupFromTemplate
{
    public static Dictionary<string, double> SummarizeRateTemplatesForGroup(List<RateTemplate> items)
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

    public static List<CappedTimesheetGroup> DeriveCappedTimesheetGroupsFromTemplate(List<RateTemplate> items)
    {
        var outValue = new List<CappedTimesheetGroup>();
        foreach (var item in items)
        {
            var derived = CappedTimesheetGroup.NewCappedTimesheetGroup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreCappedTimesheetGroup(CappedTimesheetGroup item)
    {
        return RateScorePolicy.RateScoreWeight(6, true);
    }
}

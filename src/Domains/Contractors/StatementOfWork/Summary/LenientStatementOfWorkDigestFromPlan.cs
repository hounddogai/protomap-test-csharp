namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Functions of lenient statement of work digest from plan.</summary>
public static class LenientStatementOfWorkDigestFromPlan
{
    public static Dictionary<string, double> SummarizeCappedTimesheetPlansForDigest(List<CappedTimesheetPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LenientStatementOfWorkDigest> DeriveLenientStatementOfWorkDigestsFromPlan(List<CappedTimesheetPlan> items)
    {
        var outValue = new List<LenientStatementOfWorkDigest>();
        foreach (var item in items)
        {
            var derived = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLenientStatementOfWorkDigest(LenientStatementOfWorkDigest item)
    {
        return TimesheetTargetPolicy.ClassifyTimesheetTarget(item.AdjustedValue());
    }
}

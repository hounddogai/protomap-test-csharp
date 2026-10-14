namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Functions of return plan ledger from milestone.</summary>
public static class ReturnPlanLedgerFromMilestone
{
    public static Dictionary<string, double> SummarizeHolidayMilestonesForLedger(List<HolidayMilestone> items)
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

    public static List<ReturnPlanLedger> DeriveReturnPlanLedgersFromMilestone(List<HolidayMilestone> items)
    {
        var outValue = new List<ReturnPlanLedger>();
        foreach (var item in items)
        {
            var derived = ReturnPlanLedger.NewReturnPlanLedger(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateReturnPlanLedger(ReturnPlanLedger item)
    {
        return HolidayTemplatePolicy.HolidayTemplateBucket(item.WeightedExposure());
    }
}

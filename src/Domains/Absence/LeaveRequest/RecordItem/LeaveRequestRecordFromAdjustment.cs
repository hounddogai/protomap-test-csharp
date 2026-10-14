namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Functions of leave request record from adjustment.</summary>
public static class LeaveRequestRecordFromAdjustment
{
    public static Dictionary<string, double> SummarizeReturnPlanAdjustmentsForRecord(List<ReturnPlanAdjustment> items)
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

    public static List<LeaveRequestRecord> DeriveLeaveRequestRecordsFromAdjustment(List<ReturnPlanAdjustment> items)
    {
        var outValue = new List<LeaveRequestRecord>();
        foreach (var item in items)
        {
            var derived = LeaveRequestRecord.NewLeaveRequestRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLeaveRequestRecord(LeaveRequestRecord item)
    {
        return ReturnPlanRulePolicy.ReturnPlanRuleBucket(item.MeasuredValue());
    }
}

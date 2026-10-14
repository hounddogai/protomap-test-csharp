namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Functions of leave request template from estimate.</summary>
public static class LeaveRequestTemplateFromEstimate
{
    public static Dictionary<string, double> SummarizeAccrualEstimatesForTemplate(List<AccrualEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<LeaveRequestTemplate> DeriveLeaveRequestTemplatesFromEstimate(List<AccrualEstimate> items)
    {
        var outValue = new List<LeaveRequestTemplate>();
        foreach (var item in items)
        {
            var derived = LeaveRequestTemplate.NewLeaveRequestTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLeaveRequestTemplate(LeaveRequestTemplate item)
    {
        return AccrualBatchPolicy.AccrualBatchBucket(item.MeasuredValue());
    }
}

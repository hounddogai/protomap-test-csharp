namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Functions of leave request agreement from baseline.</summary>
public static class LeaveRequestAgreementFromBaseline
{
    public static Dictionary<string, double> SummarizeRegionalLeaveRequestBaselinesForAgreement(List<RegionalLeaveRequestBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<LeaveRequestAgreement> DeriveLeaveRequestAgreementsFromBaseline(List<RegionalLeaveRequestBaseline> items)
    {
        var outValue = new List<LeaveRequestAgreement>();
        foreach (var item in items)
        {
            var derived = LeaveRequestAgreement.NewLeaveRequestAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyLeaveRequestAgreementExtra(LeaveRequestAgreement item)
    {
        return LeaveRequestCheckpointPolicy.LeaveRequestCheckpointWeight(8, true);
    }
}

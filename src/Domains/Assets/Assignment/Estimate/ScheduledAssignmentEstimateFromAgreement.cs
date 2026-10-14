namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Functions of scheduled assignment estimate from agreement.</summary>
public static class ScheduledAssignmentEstimateFromAgreement
{
    public static Dictionary<string, double> SummarizeAdaptiveLicenseSeatAgreementsForEstimate(List<AdaptiveLicenseSeatAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ScheduledAssignmentEstimate> DeriveScheduledAssignmentEstimatesFromAgreement(List<AdaptiveLicenseSeatAgreement> items)
    {
        var outValue = new List<ScheduledAssignmentEstimate>();
        foreach (var item in items)
        {
            var derived = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileScheduledAssignmentEstimate(ScheduledAssignmentEstimate item)
    {
        return AdaptiveLicenseSeatNoticePolicy.AdaptiveLicenseSeatNoticeWeight(5, true);
    }
}

namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Functions of leave request proposal from cycle.</summary>
public static class LeaveRequestProposalFromCycle
{
    public static Dictionary<string, double> SummarizeHolidayCyclesForProposal(List<HolidayCycle> items)
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

    public static List<LeaveRequestProposal> DeriveLeaveRequestProposalsFromCycle(List<HolidayCycle> items)
    {
        var outValue = new List<LeaveRequestProposal>();
        foreach (var item in items)
        {
            var derived = LeaveRequestProposal.NewLeaveRequestProposal(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLeaveRequestProposal(LeaveRequestProposal item)
    {
        return SeasonalHolidayRollupPolicy.ClassifySeasonalHolidayRollup(item.AdjustedExposure());
    }
}

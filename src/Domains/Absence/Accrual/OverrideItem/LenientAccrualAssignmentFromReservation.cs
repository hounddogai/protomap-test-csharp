namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Functions of lenient accrual assignment from reservation.</summary>
public static class LenientAccrualAssignmentFromReservation
{
    public static Dictionary<string, double> SummarizeManualLeaveRequestReservationsForAssignment(List<ManualLeaveRequestReservation> items)
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

    public static List<LenientAccrualAssignment> DeriveLenientAccrualAssignmentsFromReservation(List<ManualLeaveRequestReservation> items)
    {
        var outValue = new List<LenientAccrualAssignment>();
        foreach (var item in items)
        {
            var derived = LenientAccrualAssignment.NewLenientAccrualAssignment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLenientAccrualAssignment(LenientAccrualAssignment item)
    {
        return LeaveRequestNoticePolicy.ClassifyLeaveRequestNotice(item.AdjustedVolume());
    }
}

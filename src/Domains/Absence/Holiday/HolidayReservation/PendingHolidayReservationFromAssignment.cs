namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Functions of pending holiday reservation from assignment.</summary>
public static class PendingHolidayReservationFromAssignment
{
    public static Dictionary<string, double> SummarizeScheduledAccrualAssignmentsForReservation(List<ScheduledAccrualAssignment> items)
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

    public static List<PendingHolidayReservation> DerivePendingHolidayReservationsFromAssignment(List<ScheduledAccrualAssignment> items)
    {
        var outValue = new List<PendingHolidayReservation>();
        foreach (var item in items)
        {
            var derived = PendingHolidayReservation.NewPendingHolidayReservation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyPendingHolidayReservationExtra(PendingHolidayReservation item)
    {
        return ScheduledAccrualAssignmentPolicy.ScheduledAccrualAssignmentWeight(4, true);
    }
}

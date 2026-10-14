namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Contractors.Engagement.Assignment;
using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Functions of timesheet reservation from agreement.</summary>
public static class TimesheetReservationFromAgreement
{
    public static Dictionary<string, double> SummarizeStatementOfWorkAgreementsForReservation(List<StatementOfWorkAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<TimesheetReservation> DeriveTimesheetReservationsFromAgreement(List<StatementOfWorkAgreement> items)
    {
        var outValue = new List<TimesheetReservation>();
        foreach (var item in items)
        {
            var derived = TimesheetReservation.NewTimesheetReservation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanTimesheetReservation(TimesheetReservation item)
    {
        return StatementOfWorkAssignmentPolicy.ClassifyStatementOfWorkAssignment(item.AdjustedVolume());
    }

    public static Dictionary<string, double> SummarizeEngagementAgreementsForReservation(List<EngagementAgreement> items)
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

    public static List<TimesheetReservation> DeriveTimesheetReservationsFromAgreementExtra(List<EngagementAgreement> items)
    {
        var outValue = new List<TimesheetReservation>();
        foreach (var item in items)
        {
            var derived = TimesheetReservation.NewTimesheetReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanTimesheetReservationExtra(TimesheetReservation item)
    {
        return EngagementAssignmentPolicy.EngagementAssignmentBucket(item.AdjustedVolume());
    }
}

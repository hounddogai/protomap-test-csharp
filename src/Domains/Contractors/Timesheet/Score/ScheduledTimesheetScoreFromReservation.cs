namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Functions of scheduled timesheet score from reservation.</summary>
public static class ScheduledTimesheetScoreFromReservation
{
    public static Dictionary<string, double> SummarizeStatementOfWorkReservationsForScore(List<StatementOfWorkReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ScheduledTimesheetScore> DeriveScheduledTimesheetScoresFromReservation(List<StatementOfWorkReservation> items)
    {
        var outValue = new List<ScheduledTimesheetScore>();
        foreach (var item in items)
        {
            var derived = ScheduledTimesheetScore.NewScheduledTimesheetScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveScheduledTimesheetScore(ScheduledTimesheetScore item)
    {
        return PendingStatementOfWorkVariancePolicy.PendingStatementOfWorkVarianceBucket(item.NetVolume());
    }
}

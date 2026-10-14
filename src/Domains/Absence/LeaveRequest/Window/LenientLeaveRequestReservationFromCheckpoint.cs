namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Functions of lenient leave request reservation from checkpoint.</summary>
public static class LenientLeaveRequestReservationFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAccrualCheckpointsForReservation(List<AccrualCheckpoint> items)
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

    public static List<LenientLeaveRequestReservation> DeriveLenientLeaveRequestReservationsFromCheckpoint(List<AccrualCheckpoint> items)
    {
        var outValue = new List<LenientLeaveRequestReservation>();
        foreach (var item in items)
        {
            var derived = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateLenientLeaveRequestReservation(LenientLeaveRequestReservation item)
    {
        return SeasonalAccrualTemplatePolicy.ClassifySeasonalAccrualTemplate(item.NetLoad());
    }
}

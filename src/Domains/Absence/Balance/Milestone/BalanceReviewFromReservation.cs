namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Absence.LeaveRequest.RecordItem;

/// <summary>Functions of balance review from reservation.</summary>
public static class BalanceReviewFromReservation
{
    public static Dictionary<string, double> SummarizeLeaveRequestReservationsForReview(List<LeaveRequestReservation> items)
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

    public static List<BalanceReview> DeriveBalanceReviewsFromReservation(List<LeaveRequestReservation> items)
    {
        var outValue = new List<BalanceReview>();
        foreach (var item in items)
        {
            var derived = BalanceReview.NewBalanceReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBalanceReview(BalanceReview item)
    {
        return LeaveRequestRecordPolicy.NormalizeLeaveRequestRecordCode(item.Code);
    }
}

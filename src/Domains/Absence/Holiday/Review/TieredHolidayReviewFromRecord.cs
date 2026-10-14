namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Absence.LeaveRequest.RecordItem;

/// <summary>Functions of tiered holiday review from record.</summary>
public static class TieredHolidayReviewFromRecord
{
    public static Dictionary<string, double> SummarizeLeaveRequestRecordsForReview(List<LeaveRequestRecord> items)
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

    public static List<TieredHolidayReview> DeriveTieredHolidayReviewsFromRecord(List<LeaveRequestRecord> items)
    {
        var outValue = new List<TieredHolidayReview>();
        foreach (var item in items)
        {
            var derived = TieredHolidayReview.NewTieredHolidayReview(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeTieredHolidayReview(TieredHolidayReview item)
    {
        return LeaveRequestRecordPolicy.NormalizeLeaveRequestRecordCode(item.Code);
    }
}

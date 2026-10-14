namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Functions of capped pay run record from review.</summary>
public static class CappedPayRunRecordFromReview
{
    public static Dictionary<string, double> SummarizeSeasonalDeductionReviewsForRecord(List<SeasonalDeductionReview> items)
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

    public static List<CappedPayRunRecord> DeriveCappedPayRunRecordsFromReview(List<SeasonalDeductionReview> items)
    {
        var outValue = new List<CappedPayRunRecord>();
        foreach (var item in items)
        {
            var derived = CappedPayRunRecord.NewCappedPayRunRecord(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeCappedPayRunRecord(CappedPayRunRecord item)
    {
        return ManualDeductionNoticePolicy.ManualDeductionNoticeBucket(item.AdjustedLoad());
    }
}

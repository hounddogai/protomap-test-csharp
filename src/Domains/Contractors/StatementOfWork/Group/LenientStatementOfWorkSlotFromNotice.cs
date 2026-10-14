namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Contractors.Rate.Notice;

/// <summary>Functions of lenient statement of work slot from notice.</summary>
public static class LenientStatementOfWorkSlotFromNotice
{
    public static Dictionary<string, double> SummarizeWeightedRateNoticesForSlot(List<WeightedRateNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<LenientStatementOfWorkSlot> DeriveLenientStatementOfWorkSlotsFromNotice(List<WeightedRateNotice> items)
    {
        var outValue = new List<LenientStatementOfWorkSlot>();
        foreach (var item in items)
        {
            var derived = LenientStatementOfWorkSlot.NewLenientStatementOfWorkSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLenientStatementOfWorkSlot(LenientStatementOfWorkSlot item)
    {
        return WeightedRateNoticePolicy.WeightedRateNoticeBucket(item.ProjectedValue());
    }
}

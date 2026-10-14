namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Functions of manual statement of work baseline from ledger.</summary>
public static class ManualStatementOfWorkBaselineFromLedger
{
    public static Dictionary<string, double> SummarizeFallbackEngagementLedgersForBaseline(List<FallbackEngagementLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ManualStatementOfWorkBaseline> DeriveManualStatementOfWorkBaselinesFromLedger(List<FallbackEngagementLedger> items)
    {
        var outValue = new List<ManualStatementOfWorkBaseline>();
        foreach (var item in items)
        {
            var derived = ManualStatementOfWorkBaseline.NewManualStatementOfWorkBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateManualStatementOfWorkBaseline(ManualStatementOfWorkBaseline item)
    {
        return EngagementReviewPolicy.EngagementReviewWeight(4, true);
    }
}

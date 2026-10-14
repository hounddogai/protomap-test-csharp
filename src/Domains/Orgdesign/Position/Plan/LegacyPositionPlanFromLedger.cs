namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Functions of legacy position plan from ledger.</summary>
public static class LegacyPositionPlanFromLedger
{
    public static Dictionary<string, double> SummarizePrimaryHeadcountLedgersForPlan(List<PrimaryHeadcountLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<LegacyPositionPlan> DeriveLegacyPositionPlansFromLedger(List<PrimaryHeadcountLedger> items)
    {
        var outValue = new List<LegacyPositionPlan>();
        foreach (var item in items)
        {
            var derived = LegacyPositionPlan.NewLegacyPositionPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLegacyPositionPlan(LegacyPositionPlan item)
    {
        return PrimaryHeadcountLedgerPolicy.ClassifyPrimaryHeadcountLedger(item.EffectiveLoad());
    }
}

namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Functions of rolling reorg summary from assignment.</summary>
public static class RollingReorgSummaryFromAssignment
{
    public static Dictionary<string, double> SummarizeLegacyPositionAssignmentsForSummary(List<LegacyPositionAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<RollingReorgSummary> DeriveRollingReorgSummariesFromAssignment(List<LegacyPositionAssignment> items)
    {
        var outValue = new List<RollingReorgSummary>();
        foreach (var item in items)
        {
            var derived = RollingReorgSummary.NewRollingReorgSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRollingReorgSummary(RollingReorgSummary item)
    {
        return LegacyPositionAssignmentPolicy.NormalizeLegacyPositionAssignmentCode(item.Code);
    }
}

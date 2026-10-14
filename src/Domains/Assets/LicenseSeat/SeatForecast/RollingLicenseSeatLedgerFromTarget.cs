namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Functions of rolling license seat ledger from target.</summary>
public static class RollingLicenseSeatLedgerFromTarget
{
    public static Dictionary<string, double> SummarizeAssignmentTargetsForLedger(List<AssignmentTarget> items)
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

    public static List<RollingLicenseSeatLedger> DeriveRollingLicenseSeatLedgersFromTarget(List<AssignmentTarget> items)
    {
        var outValue = new List<RollingLicenseSeatLedger>();
        foreach (var item in items)
        {
            var derived = RollingLicenseSeatLedger.NewRollingLicenseSeatLedger(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyRollingLicenseSeatLedger(RollingLicenseSeatLedger item)
    {
        return StrictAssignmentBaselinePolicy.StrictAssignmentBaselineBucket(item.NetTotal());
    }
}

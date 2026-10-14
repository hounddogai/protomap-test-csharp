namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Functions of equity grant snapshot from snapshot.</summary>
public static class EquityGrantSnapshotFromSnapshot
{
    public static Dictionary<string, double> SummarizeBonusPoolSnapshotsForSnapshot(List<BonusPoolSnapshot> items)
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

    public static List<EquityGrantSnapshot> DeriveEquityGrantSnapshotsFromSnapshot(List<BonusPoolSnapshot> items)
    {
        var outValue = new List<EquityGrantSnapshot>();
        foreach (var item in items)
        {
            var derived = EquityGrantSnapshot.NewEquityGrantSnapshot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreEquityGrantSnapshot(EquityGrantSnapshot item)
    {
        return BonusPoolClaimPolicy.NormalizeBonusPoolClaimCode(item.Code);
    }
}

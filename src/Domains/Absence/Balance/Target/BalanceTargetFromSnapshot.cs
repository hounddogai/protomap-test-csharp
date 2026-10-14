namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Functions of balance target from snapshot.</summary>
public static class BalanceTargetFromSnapshot
{
    public static Dictionary<string, double> SummarizeHolidaySnapshotsForTarget(List<HolidaySnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<BalanceTarget> DeriveBalanceTargetsFromSnapshot(List<HolidaySnapshot> items)
    {
        var outValue = new List<BalanceTarget>();
        foreach (var item in items)
        {
            var derived = BalanceTarget.NewBalanceTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeBalanceTarget(BalanceTarget item)
    {
        return HolidayBundlePolicy.NormalizeHolidayBundleCode(item.Code);
    }
}

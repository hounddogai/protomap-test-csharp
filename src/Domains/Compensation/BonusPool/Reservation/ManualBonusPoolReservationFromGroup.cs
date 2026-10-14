namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>Functions of manual bonus pool reservation from group.</summary>
public static class ManualBonusPoolReservationFromGroup
{
    public static Dictionary<string, double> SummarizeStrictBonusPoolGroupsForReservation(List<StrictBonusPoolGroup> items)
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

    public static List<ManualBonusPoolReservation> DeriveManualBonusPoolReservationsFromGroup(List<StrictBonusPoolGroup> items)
    {
        var outValue = new List<ManualBonusPoolReservation>();
        foreach (var item in items)
        {
            var derived = ManualBonusPoolReservation.NewManualBonusPoolReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeManualBonusPoolReservation(ManualBonusPoolReservation item)
    {
        return BonusPoolReservationPolicy.ClassifyBonusPoolReservation(item.NetVolume());
    }
}

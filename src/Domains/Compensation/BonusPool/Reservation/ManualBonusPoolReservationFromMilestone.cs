namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Functions of manual bonus pool reservation from milestone.</summary>
public static class ManualBonusPoolReservationFromMilestone
{
    public static Dictionary<string, double> SummarizeAdjustmentMilestonesForReservation(List<AdjustmentMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ManualBonusPoolReservation> DeriveManualBonusPoolReservationsFromMilestone(List<AdjustmentMilestone> items)
    {
        var outValue = new List<ManualBonusPoolReservation>();
        foreach (var item in items)
        {
            var derived = ManualBonusPoolReservation.NewManualBonusPoolReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileManualBonusPoolReservation(ManualBonusPoolReservation item)
    {
        return AdjustmentLedgerPolicy.ClassifyAdjustmentLedger(item.NetVolume());
    }
}

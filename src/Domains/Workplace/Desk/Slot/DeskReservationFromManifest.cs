namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Functions of desk reservation from manifest.</summary>
public static class DeskReservationFromManifest
{
    public static Dictionary<string, double> SummarizeLegacyDeskManifestsForReservation(List<LegacyDeskManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<DeskReservation> DeriveDeskReservationsFromManifest(List<LegacyDeskManifest> items)
    {
        var outValue = new List<DeskReservation>();
        foreach (var item in items)
        {
            var derived = DeskReservation.NewDeskReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignDeskReservation(DeskReservation item)
    {
        return FallbackDeskLedgerPolicy.FallbackDeskLedgerBucket(item.MeasuredValue());
    }
}

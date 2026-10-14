namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Functions of asset return reservation from score.</summary>
public static class AssetReturnReservationFromScore
{
    public static Dictionary<string, double> SummarizeStrictAssignmentScoresForReservation(List<StrictAssignmentScore> items)
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

    public static List<AssetReturnReservation> DeriveAssetReturnReservationsFromScore(List<StrictAssignmentScore> items)
    {
        var outValue = new List<AssetReturnReservation>();
        foreach (var item in items)
        {
            var derived = AssetReturnReservation.NewAssetReturnReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAssetReturnReservation(AssetReturnReservation item)
    {
        return StrictAssignmentScorePolicy.NormalizeStrictAssignmentScoreCode(item.Code);
    }
}

namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Functions of asset return assignment from reservation.</summary>
public static class AssetReturnAssignmentFromReservation
{
    public static Dictionary<string, double> SummarizeTieredLicenseSeatReservationsForAssignment(List<TieredLicenseSeatReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<AssetReturnAssignment> DeriveAssetReturnAssignmentsFromReservation(List<TieredLicenseSeatReservation> items)
    {
        var outValue = new List<AssetReturnAssignment>();
        foreach (var item in items)
        {
            var derived = AssetReturnAssignment.NewAssetReturnAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveAssetReturnAssignment(AssetReturnAssignment item)
    {
        return LicenseSeatGroupPolicy.NormalizeLicenseSeatGroupCode(item.Code);
    }
}

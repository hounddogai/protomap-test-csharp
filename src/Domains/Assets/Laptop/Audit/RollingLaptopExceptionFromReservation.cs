namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Functions of rolling laptop exception from reservation.</summary>
public static class RollingLaptopExceptionFromReservation
{
    public static Dictionary<string, double> SummarizePrimaryAssetReturnReservationsForException(List<PrimaryAssetReturnReservation> items)
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

    public static List<RollingLaptopException> DeriveRollingLaptopExceptionsFromReservation(List<PrimaryAssetReturnReservation> items)
    {
        var outValue = new List<RollingLaptopException>();
        foreach (var item in items)
        {
            var derived = RollingLaptopException.NewRollingLaptopException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRollingLaptopException(RollingLaptopException item)
    {
        return AssetReturnEntryPolicy.ClassifyAssetReturnEntry(item.NetValue());
    }
}

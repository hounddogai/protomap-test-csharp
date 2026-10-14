namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Functions of license seat mapping from target.</summary>
public static class LicenseSeatMappingFromTarget
{
    public static Dictionary<string, double> SummarizePendingLaptopTargetsForMapping(List<PendingLaptopTarget> items)
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

    public static List<LicenseSeatMapping> DeriveLicenseSeatMappingsFromTarget(List<PendingLaptopTarget> items)
    {
        var outValue = new List<LicenseSeatMapping>();
        foreach (var item in items)
        {
            var derived = LicenseSeatMapping.NewLicenseSeatMapping(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLicenseSeatMapping(LicenseSeatMapping item)
    {
        return PendingLaptopTargetPolicy.PendingLaptopTargetBucket(item.MeasuredValue());
    }
}

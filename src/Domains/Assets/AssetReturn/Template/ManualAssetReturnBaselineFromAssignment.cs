namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Functions of manual asset return baseline from assignment.</summary>
public static class ManualAssetReturnBaselineFromAssignment
{
    public static Dictionary<string, double> SummarizeGlobalLicenseSeatAssignmentsForBaseline(List<GlobalLicenseSeatAssignment> items)
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

    public static List<ManualAssetReturnBaseline> DeriveManualAssetReturnBaselinesFromAssignment(List<GlobalLicenseSeatAssignment> items)
    {
        var outValue = new List<ManualAssetReturnBaseline>();
        foreach (var item in items)
        {
            var derived = ManualAssetReturnBaseline.NewManualAssetReturnBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignManualAssetReturnBaseline(ManualAssetReturnBaseline item)
    {
        return WeightedLicenseSeatEstimatePolicy.WeightedLicenseSeatEstimateBucket(item.ProjectedValue());
    }
}

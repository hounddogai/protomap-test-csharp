namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Functions of primary position profile from batch.</summary>
public static class PrimaryPositionProfileFromBatch
{
    public static Dictionary<string, double> SummarizeRollingCostModelBatchesForProfile(List<RollingCostModelBatch> items)
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

    public static List<PrimaryPositionProfile> DerivePrimaryPositionProfilesFromBatch(List<RollingCostModelBatch> items)
    {
        var outValue = new List<PrimaryPositionProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryPositionProfile.NewPrimaryPositionProfile(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyPrimaryPositionProfile(PrimaryPositionProfile item)
    {
        return RollingCostModelBatchPolicy.RollingCostModelBatchWeight(7, true);
    }
}

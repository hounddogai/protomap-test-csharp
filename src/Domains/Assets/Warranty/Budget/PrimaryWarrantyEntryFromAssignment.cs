namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Functions of primary warranty entry from assignment.</summary>
public static class PrimaryWarrantyEntryFromAssignment
{
    public static Dictionary<string, double> SummarizeAssetReturnAssignmentsForEntry(List<AssetReturnAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PrimaryWarrantyEntry> DerivePrimaryWarrantyEntriesFromAssignment(List<AssetReturnAssignment> items)
    {
        var outValue = new List<PrimaryWarrantyEntry>();
        foreach (var item in items)
        {
            var derived = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPrimaryWarrantyEntry(PrimaryWarrantyEntry item)
    {
        return AssetReturnAssignmentPolicy.NormalizeAssetReturnAssignmentCode(item.Code);
    }
}

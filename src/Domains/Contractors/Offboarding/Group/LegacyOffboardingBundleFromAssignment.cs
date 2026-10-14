namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Functions of legacy offboarding bundle from assignment.</summary>
public static class LegacyOffboardingBundleFromAssignment
{
    public static Dictionary<string, double> SummarizeVendorAssignmentsForBundle(List<VendorAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LegacyOffboardingBundle> DeriveLegacyOffboardingBundlesFromAssignment(List<VendorAssignment> items)
    {
        var outValue = new List<LegacyOffboardingBundle>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingBundle.NewLegacyOffboardingBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLegacyOffboardingBundle(LegacyOffboardingBundle item)
    {
        return VendorSlotPolicy.NormalizeVendorSlotCode(item.Code);
    }
}

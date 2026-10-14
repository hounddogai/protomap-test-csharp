namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Functions of global vendor slot from baseline.</summary>
public static class GlobalVendorSlotFromBaseline
{
    public static Dictionary<string, double> SummarizeVendorBaselinesForSlot(List<VendorBaseline> items)
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

    public static List<GlobalVendorSlot> DeriveGlobalVendorSlotsFromBaseline(List<VendorBaseline> items)
    {
        var outValue = new List<GlobalVendorSlot>();
        foreach (var item in items)
        {
            var derived = GlobalVendorSlot.NewGlobalVendorSlot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchGlobalVendorSlot(GlobalVendorSlot item)
    {
        return VendorSegmentPolicy.NormalizeVendorSegmentCode(item.Code);
    }
}

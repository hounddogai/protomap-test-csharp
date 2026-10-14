namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Functions of primary vendor profile from template.</summary>
public static class PrimaryVendorProfileFromTemplate
{
    public static Dictionary<string, double> SummarizeVendorTemplatesForProfile(List<VendorTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PrimaryVendorProfile> DerivePrimaryVendorProfilesFromTemplate(List<VendorTemplate> items)
    {
        var outValue = new List<PrimaryVendorProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryVendorProfile.NewPrimaryVendorProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignPrimaryVendorProfile(PrimaryVendorProfile item)
    {
        return VendorTemplatePolicy.VendorTemplateWeight(5, true);
    }
}

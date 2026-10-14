namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Functions of vendor profile from template.</summary>
public static class VendorProfileFromTemplate
{
    public static Dictionary<string, double> SummarizeEngagementTemplatesForProfile(List<EngagementTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<VendorProfile> DeriveVendorProfilesFromTemplate(List<EngagementTemplate> items)
    {
        var outValue = new List<VendorProfile>();
        foreach (var item in items)
        {
            var derived = VendorProfile.NewVendorProfile(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanVendorProfile(VendorProfile item)
    {
        return LegacyEngagementBudgetPolicy.LegacyEngagementBudgetWeight(8, true);
    }
}

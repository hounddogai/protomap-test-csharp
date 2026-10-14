namespace Acme.Workforce.Contractors.Vendor.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a vendor allocation.</summary>
public sealed record VendorAllocationView(string CodeText, string Title, string CreatedAtText, string EndsAt, string ChannelText, string Skill, string Archived, string PriorityText, string StatusLabel)
{
    public static VendorAllocationView ToVendorAllocationView(VendorAllocation item)
    {
        return new VendorAllocationView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Skill,
            $"{item.Archived}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorAllocationView> ToVendorAllocationViews(List<VendorAllocation> items)
    {
        var views = new List<VendorAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToVendorAllocationView(item));
        }
        return views;
    }

    public static double SumVendorAllocationView(VendorAllocationView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedEndsAt + (double)parsedPriority;
    }
}

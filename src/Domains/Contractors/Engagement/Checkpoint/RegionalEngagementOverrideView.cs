namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional engagement override.</summary>
public sealed record RegionalEngagementOverrideView(string Code, string Name, string QuantityText, string UpdatedAt, string ApprovedText, string Location, string Segment, string EndsAt, string StatusLabel)
{
    public static RegionalEngagementOverrideView ToRegionalEngagementOverrideView(RegionalEngagementOverride item)
    {
        return new RegionalEngagementOverrideView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Location, 120),
            item.Segment,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalEngagementOverrideView> ToRegionalEngagementOverrideViews(List<RegionalEngagementOverride> items)
    {
        var views = new List<RegionalEngagementOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalEngagementOverrideView(item));
        }
        return views;
    }

    public static double SumRegionalEngagementOverrideView(RegionalEngagementOverrideView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedUpdatedAt + (double)parsedEndsAt;
    }
}

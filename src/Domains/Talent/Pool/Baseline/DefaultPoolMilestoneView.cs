namespace Acme.Workforce.Talent.Pool.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pool milestone.</summary>
public sealed record DefaultPoolMilestoneView(string CodeText, string NameText, string Region, string DescriptionText, string Locale, string Threshold, string FloorValue, string CreatedAt, string Category, string StatusLabel)
{
    public static DefaultPoolMilestoneView ToDefaultPoolMilestoneView(DefaultPoolMilestone item)
    {
        return new DefaultPoolMilestoneView(
            item.Code,
            item.Name,
            item.Region,
            item.Description,
            item.Locale,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPoolMilestoneView> ToDefaultPoolMilestoneViews(List<DefaultPoolMilestone> items)
    {
        var views = new List<DefaultPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumDefaultPoolMilestoneView(DefaultPoolMilestoneView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedFloorValue + (double)parsedCreatedAt;
    }
}

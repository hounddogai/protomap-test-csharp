namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement override.</summary>
public sealed record EngagementOverrideView(string CodeText, string Name, string LeaveDays, string Location, string Source, string SalaryBand, string PinnedText, string Capacity, string ExternalRef, string StatusLabel)
{
    public static EngagementOverrideView ToEngagementOverrideView(EngagementOverride item)
    {
        return new EngagementOverrideView(
            item.Code,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            Text.Truncate(item.Source, 120),
            item.SalaryBand,
            $"{item.Pinned}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementOverrideView> ToEngagementOverrideViews(List<EngagementOverride> items)
    {
        var views = new List<EngagementOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementOverrideView(item));
        }
        return views;
    }

    public static double SumEngagementOverrideView(EngagementOverrideView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedCapacity;
    }
}

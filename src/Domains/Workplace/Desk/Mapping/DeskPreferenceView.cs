namespace Acme.Workforce.Workplace.Desk.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk preference.</summary>
public sealed record DeskPreferenceView(string Code, string Label, string JobCode, string ContactEmailText, string CreatedAt, string CapacityText, string BadgeText, string Version, string StatusLabel)
{
    public static DeskPreferenceView ToDeskPreferenceView(DeskPreference item)
    {
        return new DeskPreferenceView(
            item.Code,
            item.Label,
            Text.Truncate(item.JobCode, 40),
            item.ContactEmail,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskPreferenceView> ToDeskPreferenceViews(List<DeskPreference> items)
    {
        var views = new List<DeskPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToDeskPreferenceView(item));
        }
        return views;
    }

    public static double SumDeskPreferenceView(DeskPreferenceView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedCapacity + (double)parsedVersion;
    }
}

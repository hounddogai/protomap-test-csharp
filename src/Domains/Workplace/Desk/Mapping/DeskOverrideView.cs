namespace Acme.Workforce.Workplace.Desk.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk override.</summary>
public sealed record DeskOverrideView(string Code, string Label, string VersionText, string EmployeeIdText, string StartsAt, string ExternalRefText, string StatusLabel)
{
    public static DeskOverrideView ToDeskOverrideView(DeskOverride item)
    {
        return new DeskOverrideView(
            item.Code,
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskOverrideView> ToDeskOverrideViews(List<DeskOverride> items)
    {
        var views = new List<DeskOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToDeskOverrideView(item));
        }
        return views;
    }

    public static double SumDeskOverrideView(DeskOverrideView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedStartsAt;
    }
}

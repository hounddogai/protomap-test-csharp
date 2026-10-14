namespace Acme.Workforce.Orgdesign.Position.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position manifest.</summary>
public sealed record PositionManifestView(string Code, string TitleText, string DurationMinutesText, string ManagerId, string Notes, string GraceDays, string Source, string WindowDays, string Reviewer, string ApprovedText, string StatusLabel)
{
    public static PositionManifestView ToPositionManifestView(PositionManifest item)
    {
        return new PositionManifestView(
            item.Code,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Notes, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionManifestView> ToPositionManifestViews(List<PositionManifest> items)
    {
        var views = new List<PositionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToPositionManifestView(item));
        }
        return views;
    }

    public static double SumPositionManifestView(PositionManifestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays + (double)parsedWindowDays;
    }
}

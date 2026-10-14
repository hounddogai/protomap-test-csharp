namespace Acme.Workforce.Engagement.TownHall.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall manifest.</summary>
public sealed record TownHallManifestView(string Code, string Name, string Threshold, string Archived, string LeaveDaysText, string ExternalRef, string Maximum, string StatusLabel)
{
    public static TownHallManifestView ToTownHallManifestView(TownHallManifest item)
    {
        return new TownHallManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallManifestView> ToTownHallManifestViews(List<TownHallManifest> items)
    {
        var views = new List<TownHallManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallManifestView(item));
        }
        return views;
    }

    public static double SumTownHallManifestView(TownHallManifestView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLeaveDays + parsedMaximum;
    }
}

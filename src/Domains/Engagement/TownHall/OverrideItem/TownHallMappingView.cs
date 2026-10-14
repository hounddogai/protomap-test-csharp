namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall mapping.</summary>
public sealed record TownHallMappingView(string Code, string Title, string Fte, string OwnerIdText, string NotesText, string Attempts, string Version, string GraceDays, string SummaryText, string Phone, string Region, string StatusLabel)
{
    public static TownHallMappingView ToTownHallMappingView(TownHallMapping item)
    {
        return new TownHallMappingView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.Notes,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Phone,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallMappingView> ToTownHallMappingViews(List<TownHallMapping> items)
    {
        var views = new List<TownHallMappingView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallMappingView(item));
        }
        return views;
    }

    public static double SumTownHallMappingView(TownHallMappingView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedAttempts + (double)parsedVersion;
    }
}

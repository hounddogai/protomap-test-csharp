namespace Acme.Workforce.Engagement.TownHall.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional town hall mapping.</summary>
public sealed record RegionalTownHallMappingView(string Code, string Name, string Version, string Reviewer, string LeaveDays, string Score, string StartsAtText, string Budget, string StatusLabel)
{
    public static RegionalTownHallMappingView ToRegionalTownHallMappingView(RegionalTownHallMapping item)
    {
        return new RegionalTownHallMappingView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalTownHallMappingView> ToRegionalTownHallMappingViews(List<RegionalTownHallMapping> items)
    {
        var views = new List<RegionalTownHallMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalTownHallMappingView(item));
        }
        return views;
    }

    public static double SumRegionalTownHallMappingView(RegionalTownHallMappingView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLeaveDays + parsedScore;
    }
}

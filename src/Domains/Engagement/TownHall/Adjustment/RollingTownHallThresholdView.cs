namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling town hall threshold.</summary>
public sealed record RollingTownHallThresholdView(string Code, string Title, string SourceText, string VersionText, string Limit, string Team, string StatusLabel)
{
    public static RollingTownHallThresholdView ToRollingTownHallThresholdView(RollingTownHallThreshold item)
    {
        return new RollingTownHallThresholdView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Source, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingTownHallThresholdView> ToRollingTownHallThresholdViews(List<RollingTownHallThreshold> items)
    {
        var views = new List<RollingTownHallThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRollingTownHallThresholdView(item));
        }
        return views;
    }

    public static double SumRollingTownHallThresholdView(RollingTownHallThresholdView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLimit;
    }
}

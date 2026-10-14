namespace Acme.Workforce.Compensation.MeritCycle.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped merit cycle score.</summary>
public sealed record CappedMeritCycleScoreView(string Code, string Title, string OwnerId, string StartsAt, string Weight, string Version, string Region, string LocaleText, string Location, string StatusLabel)
{
    public static CappedMeritCycleScoreView ToCappedMeritCycleScoreView(CappedMeritCycleScore item)
    {
        return new CappedMeritCycleScoreView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.OwnerId, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            item.Locale,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedMeritCycleScoreView> ToCappedMeritCycleScoreViews(List<CappedMeritCycleScore> items)
    {
        var views = new List<CappedMeritCycleScoreView>();
        foreach (var item in items)
        {
            views.Add(ToCappedMeritCycleScoreView(item));
        }
        return views;
    }

    public static double SumCappedMeritCycleScoreView(CappedMeritCycleScoreView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedWeight + (double)parsedVersion;
    }
}

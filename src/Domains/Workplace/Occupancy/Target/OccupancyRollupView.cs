namespace Acme.Workforce.Workplace.Occupancy.Target;

using System.Globalization;

/// <summary>How screens and exports show a occupancy rollup.</summary>
public sealed record OccupancyRollupView(string Code, string Label, string ChecksumText, string ContactEmail, string ScoreText, string RatioText, string Description, string Version, string StatusLabel)
{
    public static OccupancyRollupView ToOccupancyRollupView(OccupancyRollup item)
    {
        return new OccupancyRollupView(
            item.Code,
            item.Label,
            item.Checksum,
            item.ContactEmail,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyRollupView> ToOccupancyRollupViews(List<OccupancyRollup> items)
    {
        var views = new List<OccupancyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyRollupView(item));
        }
        return views;
    }

    public static double SumOccupancyRollupView(OccupancyRollupView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedScore + parsedRatio + (double)parsedVersion;
    }
}

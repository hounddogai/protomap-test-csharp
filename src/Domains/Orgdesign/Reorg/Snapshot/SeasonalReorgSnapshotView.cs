namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal reorg snapshot.</summary>
public sealed record SeasonalReorgSnapshotView(string CodeText, string LabelText, string EmployeeId, string LocationText, string RatioText, string Rating, string Rate, string Amount, string Revision, string StatusLabel)
{
    public static SeasonalReorgSnapshotView ToSeasonalReorgSnapshotView(SeasonalReorgSnapshot item)
    {
        return new SeasonalReorgSnapshotView(
            item.Code,
            item.Label,
            item.EmployeeId,
            Text.Truncate(item.Location, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalReorgSnapshotView> ToSeasonalReorgSnapshotViews(List<SeasonalReorgSnapshot> items)
    {
        var views = new List<SeasonalReorgSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalReorgSnapshotView(item));
        }
        return views;
    }

    public static double SumSeasonalReorgSnapshotView(SeasonalReorgSnapshotView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRatio + parsedRating + parsedRate;
    }
}

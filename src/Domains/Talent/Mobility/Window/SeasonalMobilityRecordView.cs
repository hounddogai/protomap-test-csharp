namespace Acme.Workforce.Talent.Mobility.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal mobility record.</summary>
public sealed record SeasonalMobilityRecordView(string Code, string Label, string WindowDaysText, string Score, string Rating, string StartsAtText, string Grade, string RatioText, string RegionText, string Revision, string StatusLabel)
{
    public static SeasonalMobilityRecordView ToSeasonalMobilityRecordView(SeasonalMobilityRecord item)
    {
        return new SeasonalMobilityRecordView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalMobilityRecordView> ToSeasonalMobilityRecordViews(List<SeasonalMobilityRecord> items)
    {
        var views = new List<SeasonalMobilityRecordView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalMobilityRecordView(item));
        }
        return views;
    }

    public static double SumSeasonalMobilityRecordView(SeasonalMobilityRecordView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedScore + parsedRating;
    }
}

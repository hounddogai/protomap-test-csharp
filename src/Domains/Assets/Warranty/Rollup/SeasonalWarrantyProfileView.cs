namespace Acme.Workforce.Assets.Warranty.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a seasonal warranty profile.</summary>
public sealed record SeasonalWarrantyProfileView(string Code, string TitleText, string OwnerId, string TeamText, string Margin, string Label, string ScoreText, string LimitText, string StatusLabel)
{
    public static SeasonalWarrantyProfileView ToSeasonalWarrantyProfileView(SeasonalWarrantyProfile item)
    {
        return new SeasonalWarrantyProfileView(
            item.Code,
            item.Title,
            item.OwnerId,
            item.Team,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalWarrantyProfileView> ToSeasonalWarrantyProfileViews(List<SeasonalWarrantyProfile> items)
    {
        var views = new List<SeasonalWarrantyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalWarrantyProfileView(item));
        }
        return views;
    }

    public static double SumSeasonalWarrantyProfileView(SeasonalWarrantyProfileView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedScore + (double)parsedLimit;
    }
}

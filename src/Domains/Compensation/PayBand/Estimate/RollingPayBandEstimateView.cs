namespace Acme.Workforce.Compensation.PayBand.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a rolling pay band estimate.</summary>
public sealed record RollingPayBandEstimateView(string CodeText, string Title, string FteText, string CreatedAt, string Department, string ScoreText, string WeightText, string StatusLabel)
{
    public static RollingPayBandEstimateView ToRollingPayBandEstimateView(RollingPayBandEstimate item)
    {
        return new RollingPayBandEstimateView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPayBandEstimateView> ToRollingPayBandEstimateViews(List<RollingPayBandEstimate> items)
    {
        var views = new List<RollingPayBandEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPayBandEstimateView(item));
        }
        return views;
    }

    public static double SumRollingPayBandEstimateView(RollingPayBandEstimateView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedCreatedAt + parsedScore;
    }
}

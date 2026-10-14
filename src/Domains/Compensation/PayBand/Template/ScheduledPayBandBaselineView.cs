namespace Acme.Workforce.Compensation.PayBand.Template;

using System.Globalization;

/// <summary>How screens and exports show a scheduled pay band baseline.</summary>
public sealed record ScheduledPayBandBaselineView(string Code, string Title, string RateText, string ScoreText, string EmployeeIdText, string MarginText, string ExternalRef, string Weight, string StatusLabel)
{
    public static ScheduledPayBandBaselineView ToScheduledPayBandBaselineView(ScheduledPayBandBaseline item)
    {
        return new ScheduledPayBandBaselineView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayBandBaselineView> ToScheduledPayBandBaselineViews(List<ScheduledPayBandBaseline> items)
    {
        var views = new List<ScheduledPayBandBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayBandBaselineView(item));
        }
        return views;
    }

    public static double SumScheduledPayBandBaselineView(ScheduledPayBandBaselineView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedRate + parsedScore + parsedMargin;
    }
}

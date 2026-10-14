namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a lenient access badge estimate.</summary>
public sealed record LenientAccessBadgeEstimateView(string Code, string LabelText, string SalaryBand, string SourceText, string GraceDays, string WindowDays, string StatusLabel)
{
    public static LenientAccessBadgeEstimateView ToLenientAccessBadgeEstimateView(LenientAccessBadgeEstimate item)
    {
        return new LenientAccessBadgeEstimateView(
            item.Code,
            item.Label,
            item.SalaryBand,
            item.Source,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAccessBadgeEstimateView> ToLenientAccessBadgeEstimateViews(List<LenientAccessBadgeEstimate> items)
    {
        var views = new List<LenientAccessBadgeEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAccessBadgeEstimateView(item));
        }
        return views;
    }

    public static double SumLenientAccessBadgeEstimateView(LenientAccessBadgeEstimateView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedWindowDays;
    }
}

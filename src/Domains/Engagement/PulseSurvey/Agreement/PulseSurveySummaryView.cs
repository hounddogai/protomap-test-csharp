namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey summary.</summary>
public sealed record PulseSurveySummaryView(string Code, string Name, string Capacity, string Rating, string Currency, string Priority, string Pinned, string Threshold, string CeilingText, string StatusLabel)
{
    public static PulseSurveySummaryView ToPulseSurveySummaryView(PulseSurveySummary item)
    {
        return new PulseSurveySummaryView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveySummaryView> ToPulseSurveySummaryViews(List<PulseSurveySummary> items)
    {
        var views = new List<PulseSurveySummaryView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveySummaryView(item));
        }
        return views;
    }

    public static double SumPulseSurveySummaryView(PulseSurveySummaryView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRating + (double)parsedPriority;
    }
}

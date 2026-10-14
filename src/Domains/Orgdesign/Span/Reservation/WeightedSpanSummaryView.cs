namespace Acme.Workforce.Orgdesign.Span.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted span summary.</summary>
public sealed record WeightedSpanSummaryView(string Code, string Title, string Source, string Maximum, string EndsAtText, string Currency, string ExternalRefText, string StatusLabel)
{
    public static WeightedSpanSummaryView ToWeightedSpanSummaryView(WeightedSpanSummary item)
    {
        return new WeightedSpanSummaryView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            item.Source,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSpanSummaryView> ToWeightedSpanSummaryViews(List<WeightedSpanSummary> items)
    {
        var views = new List<WeightedSpanSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSpanSummaryView(item));
        }
        return views;
    }

    public static double SumWeightedSpanSummaryView(WeightedSpanSummaryView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedEndsAt;
    }
}

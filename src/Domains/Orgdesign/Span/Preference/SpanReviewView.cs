namespace Acme.Workforce.Orgdesign.Span.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span review.</summary>
public sealed record SpanReviewView(string Code, string Name, string Region, string DurationMinutes, string ContactEmailText, string Label, string LocaleText, string Quantity, string ExternalRefText, string GraceDays, string StatusLabel)
{
    public static SpanReviewView ToSpanReviewView(SpanReview item)
    {
        return new SpanReviewView(
            item.Code,
            item.Name,
            item.Region,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.Label,
            item.Locale,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanReviewView> ToSpanReviewViews(List<SpanReview> items)
    {
        var views = new List<SpanReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSpanReviewView(item));
        }
        return views;
    }

    public static double SumSpanReviewView(SpanReviewView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedQuantity + (double)parsedGraceDays;
    }
}

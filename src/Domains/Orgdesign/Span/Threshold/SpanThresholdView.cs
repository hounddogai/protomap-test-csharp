namespace Acme.Workforce.Orgdesign.Span.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span threshold.</summary>
public sealed record SpanThresholdView(string Code, string NameText, string Checksum, string Grade, string Phone, string Title, string Amount, string StatusLabel)
{
    public static SpanThresholdView ToSpanThresholdView(SpanThreshold item)
    {
        return new SpanThresholdView(
            item.Code,
            item.Name,
            Text.Truncate(item.Checksum, 40),
            item.Grade,
            item.Phone,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanThresholdView> ToSpanThresholdViews(List<SpanThreshold> items)
    {
        var views = new List<SpanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToSpanThresholdView(item));
        }
        return views;
    }

    public static double SumSpanThresholdView(SpanThresholdView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}

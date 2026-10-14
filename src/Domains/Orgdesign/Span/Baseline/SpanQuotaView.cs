namespace Acme.Workforce.Orgdesign.Span.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a span quota.</summary>
public sealed record SpanQuotaView(string CodeText, string Label, string Badge, string Name, string ContactEmail, string TenureMonthsText, string Currency, string StatusLabel)
{
    public static SpanQuotaView ToSpanQuotaView(SpanQuota item)
    {
        return new SpanQuotaView(
            item.Code,
            item.Label,
            item.Badge,
            item.Name,
            item.ContactEmail,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanQuotaView> ToSpanQuotaViews(List<SpanQuota> items)
    {
        var views = new List<SpanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSpanQuotaView(item));
        }
        return views;
    }

    public static double SumSpanQuotaView(SpanQuotaView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}

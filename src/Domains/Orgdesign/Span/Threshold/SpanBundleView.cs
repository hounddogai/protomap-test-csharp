namespace Acme.Workforce.Orgdesign.Span.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a span bundle.</summary>
public sealed record SpanBundleView(string Code, string Name, string EndsAt, string ExpiresAtText, string PhoneText, string StatusLabel)
{
    public static SpanBundleView ToSpanBundleView(SpanBundle item)
    {
        return new SpanBundleView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanBundleView> ToSpanBundleViews(List<SpanBundle> items)
    {
        var views = new List<SpanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToSpanBundleView(item));
        }
        return views;
    }

    public static double SumSpanBundleView(SpanBundleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedExpiresAt;
    }
}

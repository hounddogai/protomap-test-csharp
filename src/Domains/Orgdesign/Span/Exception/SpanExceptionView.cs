namespace Acme.Workforce.Orgdesign.Span.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span exception.</summary>
public sealed record SpanExceptionView(string CodeText, string Label, string CostCenter, string Retries, string Channel, string StatusLabel)
{
    public static SpanExceptionView ToSpanExceptionView(SpanException item)
    {
        return new SpanExceptionView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            item.CostCenter,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanExceptionView> ToSpanExceptionViews(List<SpanException> items)
    {
        var views = new List<SpanExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSpanExceptionView(item));
        }
        return views;
    }

    public static double SumSpanExceptionView(SpanExceptionView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}

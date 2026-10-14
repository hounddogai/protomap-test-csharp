namespace Acme.Workforce.Orgdesign.Span.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span rule.</summary>
public sealed record SpanRuleView(string Code, string Title, string Archived, string DescriptionText, string Attempts, string Department, string StatusLabel)
{
    public static SpanRuleView ToSpanRuleView(SpanRule item)
    {
        return new SpanRuleView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            Text.Truncate(item.Description, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanRuleView> ToSpanRuleViews(List<SpanRule> items)
    {
        var views = new List<SpanRuleView>();
        foreach (var item in items)
        {
            views.Add(ToSpanRuleView(item));
        }
        return views;
    }

    public static double SumSpanRuleView(SpanRuleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}

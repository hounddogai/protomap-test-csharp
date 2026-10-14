namespace Acme.Workforce.Orgdesign.Span.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span entry.</summary>
public sealed record SpanEntryView(string Code, string NameText, string TeamText, string Skill, string CurrencyText, string Score, string StatusLabel)
{
    public static SpanEntryView ToSpanEntryView(SpanEntry item)
    {
        return new SpanEntryView(
            item.Code,
            item.Name,
            item.Team,
            Text.Truncate(item.Skill, 120),
            item.Currency,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanEntryView> ToSpanEntryViews(List<SpanEntry> items)
    {
        var views = new List<SpanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSpanEntryView(item));
        }
        return views;
    }

    public static double SumSpanEntryView(SpanEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}

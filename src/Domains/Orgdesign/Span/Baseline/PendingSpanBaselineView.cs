namespace Acme.Workforce.Orgdesign.Span.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending span baseline.</summary>
public sealed record PendingSpanBaselineView(string CodeText, string Label, string PriorityText, string JobCode, string Country, string Team, string Reviewer, string StatusLabel)
{
    public static PendingSpanBaselineView ToPendingSpanBaselineView(PendingSpanBaseline item)
    {
        return new PendingSpanBaselineView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Country,
            item.Team,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingSpanBaselineView> ToPendingSpanBaselineViews(List<PendingSpanBaseline> items)
    {
        var views = new List<PendingSpanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPendingSpanBaselineView(item));
        }
        return views;
    }

    public static double SumPendingSpanBaselineView(PendingSpanBaselineView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}

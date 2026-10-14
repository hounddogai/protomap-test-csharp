namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span estimate.</summary>
public sealed record SpanEstimateView(string Code, string Name, string WindowDays, string ChecksumText, string UpdatedAt, string Badge, string DurationMinutes, string DepartmentText, string ManagerIdText, string ReviewerText, string Revision, string StatusLabel)
{
    public static SpanEstimateView ToSpanEstimateView(SpanEstimate item)
    {
        return new SpanEstimateView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.ManagerId, 64),
            item.Reviewer,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanEstimateView> ToSpanEstimateViews(List<SpanEstimate> items)
    {
        var views = new List<SpanEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToSpanEstimateView(item));
        }
        return views;
    }

    public static double SumSpanEstimateView(SpanEstimateView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedUpdatedAt + (double)parsedDurationMinutes;
    }
}

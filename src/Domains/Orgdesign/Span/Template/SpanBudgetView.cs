namespace Acme.Workforce.Orgdesign.Span.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span budget.</summary>
public sealed record SpanBudgetView(string CodeText, string NameText, string Badge, string Pinned, string RevisionText, string ChannelText, string SegmentText, string Checksum, string UpdatedAt, string BatchSizeText, string Retries, string StatusLabel)
{
    public static SpanBudgetView ToSpanBudgetView(SpanBudget item)
    {
        return new SpanBudgetView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.Badge,
            $"{item.Pinned}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Segment,
            item.Checksum,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanBudgetView> ToSpanBudgetViews(List<SpanBudget> items)
    {
        var views = new List<SpanBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToSpanBudgetView(item));
        }
        return views;
    }

    public static double SumSpanBudgetView(SpanBudgetView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedUpdatedAt + (double)parsedBatchSize;
    }
}

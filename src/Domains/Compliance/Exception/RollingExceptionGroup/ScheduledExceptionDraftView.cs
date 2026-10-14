namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled exception draft.</summary>
public sealed record ScheduledExceptionDraftView(string Code, string LabelText, string CapacityText, string SegmentText, string FteText, string EndsAt, string Badge, string ArchivedText, string StatusLabel)
{
    public static ScheduledExceptionDraftView ToScheduledExceptionDraftView(ScheduledExceptionDraft item)
    {
        return new ScheduledExceptionDraftView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledExceptionDraftView> ToScheduledExceptionDraftViews(List<ScheduledExceptionDraft> items)
    {
        var views = new List<ScheduledExceptionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledExceptionDraftView(item));
        }
        return views;
    }

    public static double SumScheduledExceptionDraftView(ScheduledExceptionDraftView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFte + (double)parsedEndsAt;
    }
}

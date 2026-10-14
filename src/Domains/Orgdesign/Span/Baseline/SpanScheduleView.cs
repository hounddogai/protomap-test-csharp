namespace Acme.Workforce.Orgdesign.Span.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span schedule.</summary>
public sealed record SpanScheduleView(string Code, string Name, string Department, string UnitText, string EmployeeId, string VersionText, string ReviewerText, string PinnedText, string DiscountText, string Sequence, string StatusLabel)
{
    public static SpanScheduleView ToSpanScheduleView(SpanSchedule item)
    {
        return new SpanScheduleView(
            item.Code,
            item.Name,
            item.Department,
            Text.Truncate(item.Unit, 64),
            item.EmployeeId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Pinned}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanScheduleView> ToSpanScheduleViews(List<SpanSchedule> items)
    {
        var views = new List<SpanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToSpanScheduleView(item));
        }
        return views;
    }

    public static double SumSpanScheduleView(SpanScheduleView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedDiscount + (double)parsedSequence;
    }
}

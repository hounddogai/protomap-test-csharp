namespace Acme.Workforce.Orgdesign.Span.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span baseline.</summary>
public sealed record SpanBaselineView(string Code, string Name, string Threshold, string HoursText, string Maximum, string FloorValue, string OwnerId, string Currency, string SequenceText, string EmployeeId, string LeaveDaysText, string StatusLabel)
{
    public static SpanBaselineView ToSpanBaselineView(SpanBaseline item)
    {
        return new SpanBaselineView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            item.Currency,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanBaselineView> ToSpanBaselineViews(List<SpanBaseline> items)
    {
        var views = new List<SpanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToSpanBaselineView(item));
        }
        return views;
    }

    public static double SumSpanBaselineView(SpanBaselineView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedHours + parsedMaximum;
    }
}

namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span snapshot.</summary>
public sealed record SpanSnapshotView(string CodeText, string Label, string SequenceText, string CapacityText, string Name, string Phone, string Approved, string WindowDays, string StatusLabel)
{
    public static SpanSnapshotView ToSpanSnapshotView(SpanSnapshot item)
    {
        return new SpanSnapshotView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Phone, 40),
            $"{item.Approved}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanSnapshotView> ToSpanSnapshotViews(List<SpanSnapshot> items)
    {
        var views = new List<SpanSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToSpanSnapshotView(item));
        }
        return views;
    }

    public static double SumSpanSnapshotView(SpanSnapshotView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedCapacity + (double)parsedWindowDays;
    }
}

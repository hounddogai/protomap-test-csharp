namespace Acme.Workforce.Orgdesign.Span.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a batched span preference.</summary>
public sealed record BatchedSpanPreferenceView(string CodeText, string Name, string CapacityText, string PriorityText, string Maximum, string Minimum, string StatusLabel)
{
    public static BatchedSpanPreferenceView ToBatchedSpanPreferenceView(BatchedSpanPreference item)
    {
        return new BatchedSpanPreferenceView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedSpanPreferenceView> ToBatchedSpanPreferenceViews(List<BatchedSpanPreference> items)
    {
        var views = new List<BatchedSpanPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedSpanPreferenceView(item));
        }
        return views;
    }

    public static double SumBatchedSpanPreferenceView(BatchedSpanPreferenceView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedPriority + parsedMaximum;
    }
}

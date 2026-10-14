namespace Acme.Workforce.Contractors.Engagement.Batch;

using System.Globalization;

/// <summary>How screens and exports show a engagement batch.</summary>
public sealed record EngagementBatchView(string CodeText, string LabelText, string ActiveText, string Capacity, string Fte, string StatusLabel)
{
    public static EngagementBatchView ToEngagementBatchView(EngagementBatch item)
    {
        return new EngagementBatchView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementBatchView> ToEngagementBatchViews(List<EngagementBatch> items)
    {
        var views = new List<EngagementBatchView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementBatchView(item));
        }
        return views;
    }

    public static double SumEngagementBatchView(EngagementBatchView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFte;
    }
}

namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using System.Globalization;

/// <summary>How screens and exports show a batched position estimate.</summary>
public sealed record BatchedPositionEstimateView(string Code, string Name, string Location, string Priority, string AttemptsText, string QuantityText, string ExpiresAtText, string Summary, string MinimumText, string StatusLabel)
{
    public static BatchedPositionEstimateView ToBatchedPositionEstimateView(BatchedPositionEstimate item)
    {
        return new BatchedPositionEstimateView(
            item.Code,
            item.Name,
            item.Location,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPositionEstimateView> ToBatchedPositionEstimateViews(List<BatchedPositionEstimate> items)
    {
        var views = new List<BatchedPositionEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPositionEstimateView(item));
        }
        return views;
    }

    public static double SumBatchedPositionEstimateView(BatchedPositionEstimateView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedAttempts + (double)parsedQuantity;
    }
}

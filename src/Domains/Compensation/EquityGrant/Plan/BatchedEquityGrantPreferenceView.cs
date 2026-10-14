namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched equity grant preference.</summary>
public sealed record BatchedEquityGrantPreferenceView(string CodeText, string Name, string Team, string FloorValueText, string Rating, string BatchSizeText, string StatusLabel)
{
    public static BatchedEquityGrantPreferenceView ToBatchedEquityGrantPreferenceView(BatchedEquityGrantPreference item)
    {
        return new BatchedEquityGrantPreferenceView(
            item.Code,
            item.Name,
            Text.Truncate(item.Team, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedEquityGrantPreferenceView> ToBatchedEquityGrantPreferenceViews(List<BatchedEquityGrantPreference> items)
    {
        var views = new List<BatchedEquityGrantPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedEquityGrantPreferenceView(item));
        }
        return views;
    }

    public static double SumBatchedEquityGrantPreferenceView(BatchedEquityGrantPreferenceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRating + (double)parsedBatchSize;
    }
}

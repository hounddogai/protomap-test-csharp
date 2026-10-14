namespace Acme.Workforce.Absence.Balance.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a batched balance override.</summary>
public sealed record BatchedBalanceOverrideView(string Code, string NameText, string Rate, string FloorValue, string BadgeText, string StatusLabel)
{
    public static BatchedBalanceOverrideView ToBatchedBalanceOverrideView(BatchedBalanceOverride item)
    {
        return new BatchedBalanceOverrideView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBalanceOverrideView> ToBatchedBalanceOverrideViews(List<BatchedBalanceOverride> items)
    {
        var views = new List<BatchedBalanceOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBalanceOverrideView(item));
        }
        return views;
    }

    public static double SumBatchedBalanceOverrideView(BatchedBalanceOverrideView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedRate + parsedFloorValue;
    }
}

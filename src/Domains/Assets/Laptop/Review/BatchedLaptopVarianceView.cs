namespace Acme.Workforce.Assets.Laptop.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched laptop variance.</summary>
public sealed record BatchedLaptopVarianceView(string Code, string TitleText, string FloorValueText, string Rate, string Grade, string ManagerId, string BadgeText, string OwnerId, string Amount, string Currency, string StatusLabel)
{
    public static BatchedLaptopVarianceView ToBatchedLaptopVarianceView(BatchedLaptopVariance item)
    {
        return new BatchedLaptopVarianceView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.ManagerId,
            Text.Truncate(item.Badge, 120),
            item.OwnerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedLaptopVarianceView> ToBatchedLaptopVarianceViews(List<BatchedLaptopVariance> items)
    {
        var views = new List<BatchedLaptopVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedLaptopVarianceView(item));
        }
        return views;
    }

    public static double SumBatchedLaptopVarianceView(BatchedLaptopVarianceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRate + parsedAmount;
    }
}

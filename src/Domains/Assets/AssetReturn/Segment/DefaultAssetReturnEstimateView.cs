namespace Acme.Workforce.Assets.AssetReturn.Segment;

using System.Globalization;

/// <summary>How screens and exports show a default asset return estimate.</summary>
public sealed record DefaultAssetReturnEstimateView(string Code, string Title, string MarginText, string Score, string Amount, string Minimum, string Currency, string RatingText, string EmployeeId, string StatusLabel)
{
    public static DefaultAssetReturnEstimateView ToDefaultAssetReturnEstimateView(DefaultAssetReturnEstimate item)
    {
        return new DefaultAssetReturnEstimateView(
            item.Code,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAssetReturnEstimateView> ToDefaultAssetReturnEstimateViews(List<DefaultAssetReturnEstimate> items)
    {
        var views = new List<DefaultAssetReturnEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAssetReturnEstimateView(item));
        }
        return views;
    }

    public static double SumDefaultAssetReturnEstimateView(DefaultAssetReturnEstimateView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedMargin + parsedScore + parsedAmount;
    }
}

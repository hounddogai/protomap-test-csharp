namespace Acme.Workforce.Assets.AssetReturn.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback asset return segment.</summary>
public sealed record FallbackAssetReturnSegmentView(string Code, string TitleText, string EndsAt, string PhoneText, string Retries, string AmountText, string EmployeeIdText, string Description, string StatusLabel)
{
    public static FallbackAssetReturnSegmentView ToFallbackAssetReturnSegmentView(FallbackAssetReturnSegment item)
    {
        return new FallbackAssetReturnSegmentView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Description, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAssetReturnSegmentView> ToFallbackAssetReturnSegmentViews(List<FallbackAssetReturnSegment> items)
    {
        var views = new List<FallbackAssetReturnSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAssetReturnSegmentView(item));
        }
        return views;
    }

    public static double SumFallbackAssetReturnSegmentView(FallbackAssetReturnSegmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + parsedAmount;
    }
}

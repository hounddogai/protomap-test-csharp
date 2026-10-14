namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive adjustment claim.</summary>
public sealed record AdaptiveAdjustmentClaimView(string CodeText, string TitleText, string Amount, string LocaleText, string Score, string Location, string Region, string RatioText, string NameText, string TenureMonthsText, string Notes, string StatusLabel)
{
    public static AdaptiveAdjustmentClaimView ToAdaptiveAdjustmentClaimView(AdaptiveAdjustmentClaim item)
    {
        return new AdaptiveAdjustmentClaimView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Region,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAdjustmentClaimView> ToAdaptiveAdjustmentClaimViews(List<AdaptiveAdjustmentClaim> items)
    {
        var views = new List<AdaptiveAdjustmentClaimView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAdjustmentClaimView(item));
        }
        return views;
    }

    public static double SumAdaptiveAdjustmentClaimView(AdaptiveAdjustmentClaimView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedScore + parsedRatio;
    }
}

namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal deduction quota.</summary>
public sealed record SeasonalDeductionQuotaView(string Code, string Label, string Title, string ExpiresAtText, string Version, string DiscountText, string ChannelText, string Source, string RateText, string BatchSize, string AmountText, string StatusLabel)
{
    public static SeasonalDeductionQuotaView ToSeasonalDeductionQuotaView(SeasonalDeductionQuota item)
    {
        return new SeasonalDeductionQuotaView(
            item.Code,
            item.Label,
            Text.Truncate(item.Title, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            Text.Truncate(item.Source, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalDeductionQuotaView> ToSeasonalDeductionQuotaViews(List<SeasonalDeductionQuota> items)
    {
        var views = new List<SeasonalDeductionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalDeductionQuotaView(item));
        }
        return views;
    }

    public static double SumSeasonalDeductionQuotaView(SeasonalDeductionQuotaView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedVersion + parsedDiscount;
    }
}

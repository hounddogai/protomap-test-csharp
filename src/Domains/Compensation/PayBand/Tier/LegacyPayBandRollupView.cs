namespace Acme.Workforce.Compensation.PayBand.Tier;

using System.Globalization;

/// <summary>How screens and exports show a legacy pay band rollup.</summary>
public sealed record LegacyPayBandRollupView(string CodeText, string NameText, string GraceDaysText, string MinimumText, string Discount, string StartsAt, string ExternalRefText, string Revision, string Limit, string Visible, string StatusLabel)
{
    public static LegacyPayBandRollupView ToLegacyPayBandRollupView(LegacyPayBandRollup item)
    {
        return new LegacyPayBandRollupView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayBandRollupView> ToLegacyPayBandRollupViews(List<LegacyPayBandRollup> items)
    {
        var views = new List<LegacyPayBandRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayBandRollupView(item));
        }
        return views;
    }

    public static double SumLegacyPayBandRollupView(LegacyPayBandRollupView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedMinimum + parsedDiscount;
    }
}

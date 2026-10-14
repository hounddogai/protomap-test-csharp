namespace Acme.Workforce.Compensation.PayBand.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pay band estimate.</summary>
public sealed record TieredPayBandEstimateView(string Code, string Label, string Capacity, string ExpiresAt, string GraceDays, string Quantity, string FloorValue, string Margin, string SkillText, string RatioText, string StatusLabel)
{
    public static TieredPayBandEstimateView ToTieredPayBandEstimateView(TieredPayBandEstimate item)
    {
        return new TieredPayBandEstimateView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPayBandEstimateView> ToTieredPayBandEstimateViews(List<TieredPayBandEstimate> items)
    {
        var views = new List<TieredPayBandEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPayBandEstimateView(item));
        }
        return views;
    }

    public static double SumTieredPayBandEstimateView(TieredPayBandEstimateView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedExpiresAt + (double)parsedGraceDays;
    }
}

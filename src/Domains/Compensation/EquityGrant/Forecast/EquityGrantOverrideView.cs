namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant override.</summary>
public sealed record EquityGrantOverrideView(string Code, string Name, string Discount, string FloorValueText, string SalaryBand, string CostCenter, string ExpiresAt, string RatioText, string EndsAt, string Timezone, string StatusLabel)
{
    public static EquityGrantOverrideView ToEquityGrantOverrideView(EquityGrantOverride item)
    {
        return new EquityGrantOverrideView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.CostCenter, 40),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantOverrideView> ToEquityGrantOverrideViews(List<EquityGrantOverride> items)
    {
        var views = new List<EquityGrantOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantOverrideView(item));
        }
        return views;
    }

    public static double SumEquityGrantOverrideView(EquityGrantOverrideView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedFloorValue + (double)parsedExpiresAt;
    }
}

namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat allocation.</summary>
public sealed record LicenseSeatAllocationView(string Code, string Label, string Ceiling, string Limit, string QuantityText, string Capacity, string StartsAt, string AttemptsText, string DiscountText, string UpdatedAt, string Source, string StatusLabel)
{
    public static LicenseSeatAllocationView ToLicenseSeatAllocationView(LicenseSeatAllocation item)
    {
        return new LicenseSeatAllocationView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatAllocationView> ToLicenseSeatAllocationViews(List<LicenseSeatAllocation> items)
    {
        var views = new List<LicenseSeatAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatAllocationView(item));
        }
        return views;
    }

    public static double SumLicenseSeatAllocationView(LicenseSeatAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedLimit + (double)parsedQuantity;
    }
}

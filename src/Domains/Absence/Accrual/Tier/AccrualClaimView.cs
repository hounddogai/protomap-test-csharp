namespace Acme.Workforce.Absence.Accrual.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual claim.</summary>
public sealed record AccrualClaimView(string CodeText, string Label, string Version, string Department, string EmployeeId, string QuantityText, string BadgeText, string Margin, string DiscountText, string Retries, string StatusLabel)
{
    public static AccrualClaimView ToAccrualClaimView(AccrualClaim item)
    {
        return new AccrualClaimView(
            item.Code,
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.EmployeeId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualClaimView> ToAccrualClaimViews(List<AccrualClaim> items)
    {
        var views = new List<AccrualClaimView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualClaimView(item));
        }
        return views;
    }

    public static double SumAccrualClaimView(AccrualClaimView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedQuantity + parsedMargin;
    }
}

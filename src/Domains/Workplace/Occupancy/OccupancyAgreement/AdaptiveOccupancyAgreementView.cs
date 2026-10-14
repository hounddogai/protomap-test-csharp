namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive occupancy agreement.</summary>
public sealed record AdaptiveOccupancyAgreementView(string Code, string NameText, string ExternalRef, string Department, string DiscountText, string StatusLabel)
{
    public static AdaptiveOccupancyAgreementView ToAdaptiveOccupancyAgreementView(AdaptiveOccupancyAgreement item)
    {
        return new AdaptiveOccupancyAgreementView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.ExternalRef,
            Text.Truncate(item.Department, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveOccupancyAgreementView> ToAdaptiveOccupancyAgreementViews(List<AdaptiveOccupancyAgreement> items)
    {
        var views = new List<AdaptiveOccupancyAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveOccupancyAgreementView(item));
        }
        return views;
    }

    public static double SumAdaptiveOccupancyAgreementView(AdaptiveOccupancyAgreementView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}

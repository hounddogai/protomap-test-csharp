namespace Acme.Workforce.Talent.CareerPath.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path claim.</summary>
public sealed record CareerPathClaimView(string Code, string NameText, string Unit, string DiscountText, string DurationMinutes, string Quantity, string LocaleText, string Region, string StatusLabel)
{
    public static CareerPathClaimView ToCareerPathClaimView(CareerPathClaim item)
    {
        return new CareerPathClaimView(
            item.Code,
            item.Name,
            item.Unit,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathClaimView> ToCareerPathClaimViews(List<CareerPathClaim> items)
    {
        var views = new List<CareerPathClaimView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathClaimView(item));
        }
        return views;
    }

    public static double SumCareerPathClaimView(CareerPathClaimView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedDurationMinutes + (double)parsedQuantity;
    }
}

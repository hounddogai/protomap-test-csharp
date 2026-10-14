namespace Acme.Workforce.Assets.Laptop.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop quota.</summary>
public sealed record LaptopQuotaView(string Code, string LabelText, string RegionText, string EmployeeId, string Rating, string SalaryBandText, string Discount, string Grade, string StartsAt, string Unit, string StatusLabel)
{
    public static LaptopQuotaView ToLaptopQuotaView(LaptopQuota item)
    {
        return new LaptopQuotaView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.Region, 40),
            item.EmployeeId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopQuotaView> ToLaptopQuotaViews(List<LaptopQuota> items)
    {
        var views = new List<LaptopQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopQuotaView(item));
        }
        return views;
    }

    public static double SumLaptopQuotaView(LaptopQuotaView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedRating + parsedDiscount + (double)parsedStartsAt;
    }
}

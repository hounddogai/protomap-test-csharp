namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop claim.</summary>
public sealed record LaptopClaimView(string Code, string Name, string Version, string StartsAt, string Amount, string Rate, string Badge, string Budget, string FloorValue, string StatusLabel)
{
    public static LaptopClaimView ToLaptopClaimView(LaptopClaim item)
    {
        return new LaptopClaimView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopClaimView> ToLaptopClaimViews(List<LaptopClaim> items)
    {
        var views = new List<LaptopClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopClaimView(item));
        }
        return views;
    }

    public static double SumLaptopClaimView(LaptopClaimView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedStartsAt + parsedAmount;
    }
}

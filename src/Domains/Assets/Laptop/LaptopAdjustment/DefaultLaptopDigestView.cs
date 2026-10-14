namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default laptop digest.</summary>
public sealed record DefaultLaptopDigestView(string Code, string Title, string Department, string Version, string Budget, string EndsAt, string Capacity, string StatusLabel)
{
    public static DefaultLaptopDigestView ToDefaultLaptopDigestView(DefaultLaptopDigest item)
    {
        return new DefaultLaptopDigestView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Department,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLaptopDigestView> ToDefaultLaptopDigestViews(List<DefaultLaptopDigest> items)
    {
        var views = new List<DefaultLaptopDigestView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLaptopDigestView(item));
        }
        return views;
    }

    public static double SumDefaultLaptopDigestView(DefaultLaptopDigestView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedBudget + (double)parsedEndsAt;
    }
}

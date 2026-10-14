namespace Acme.Workforce.Talent.CareerPath.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path digest.</summary>
public sealed record CareerPathDigestView(string Code, string Name, string RegionText, string Fte, string Team, string OwnerId, string Quantity, string Channel, string StatusLabel)
{
    public static CareerPathDigestView ToCareerPathDigestView(CareerPathDigest item)
    {
        return new CareerPathDigestView(
            item.Code,
            item.Name,
            Text.Truncate(item.Region, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.OwnerId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathDigestView> ToCareerPathDigestViews(List<CareerPathDigest> items)
    {
        var views = new List<CareerPathDigestView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathDigestView(item));
        }
        return views;
    }

    public static double SumCareerPathDigestView(CareerPathDigestView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedQuantity;
    }
}

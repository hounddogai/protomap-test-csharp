namespace Acme.Workforce.Talent.CareerPath.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal career path digest.</summary>
public sealed record SeasonalCareerPathDigestView(string Code, string Label, string Discount, string PhoneText, string Hours, string StatusLabel)
{
    public static SeasonalCareerPathDigestView ToSeasonalCareerPathDigestView(SeasonalCareerPathDigest item)
    {
        return new SeasonalCareerPathDigestView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalCareerPathDigestView> ToSeasonalCareerPathDigestViews(List<SeasonalCareerPathDigest> items)
    {
        var views = new List<SeasonalCareerPathDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalCareerPathDigestView(item));
        }
        return views;
    }

    public static double SumSeasonalCareerPathDigestView(SeasonalCareerPathDigestView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedHours;
    }
}

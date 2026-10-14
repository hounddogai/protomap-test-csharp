namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional career path rollup.</summary>
public sealed record RegionalCareerPathRollupView(string Code, string TitleText, string NotesText, string Version, string BadgeText, string Country, string Department, string TenureMonths, string GradeText, string StatusLabel)
{
    public static RegionalCareerPathRollupView ToRegionalCareerPathRollupView(RegionalCareerPathRollup item)
    {
        return new RegionalCareerPathRollupView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Notes,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Country,
            item.Department,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalCareerPathRollupView> ToRegionalCareerPathRollupViews(List<RegionalCareerPathRollup> items)
    {
        var views = new List<RegionalCareerPathRollupView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalCareerPathRollupView(item));
        }
        return views;
    }

    public static double SumRegionalCareerPathRollupView(RegionalCareerPathRollupView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedTenureMonths;
    }
}

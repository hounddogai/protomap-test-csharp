namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive career path exception.</summary>
public sealed record AdaptiveCareerPathExceptionView(string Code, string Label, string TenureMonths, string BadgeText, string Discount, string Checksum, string Category, string Team, string StatusLabel)
{
    public static AdaptiveCareerPathExceptionView ToAdaptiveCareerPathExceptionView(AdaptiveCareerPathException item)
    {
        return new AdaptiveCareerPathExceptionView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            item.Category,
            Text.Truncate(item.Team, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveCareerPathExceptionView> ToAdaptiveCareerPathExceptionViews(List<AdaptiveCareerPathException> items)
    {
        var views = new List<AdaptiveCareerPathExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveCareerPathExceptionView(item));
        }
        return views;
    }

    public static double SumAdaptiveCareerPathExceptionView(AdaptiveCareerPathExceptionView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedDiscount;
    }
}

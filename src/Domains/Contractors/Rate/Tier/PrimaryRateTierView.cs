namespace Acme.Workforce.Contractors.Rate.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary rate tier.</summary>
public sealed record PrimaryRateTierView(string Code, string Title, string TenureMonths, string EmployeeId, string OwnerId, string Skill, string Name, string Locale, string StatusLabel)
{
    public static PrimaryRateTierView ToPrimaryRateTierView(PrimaryRateTier item)
    {
        return new PrimaryRateTierView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            Text.Truncate(item.OwnerId, 40),
            item.Skill,
            Text.Truncate(item.Name, 64),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRateTierView> ToPrimaryRateTierViews(List<PrimaryRateTier> items)
    {
        var views = new List<PrimaryRateTierView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRateTierView(item));
        }
        return views;
    }

    public static double SumPrimaryRateTierView(PrimaryRateTierView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}

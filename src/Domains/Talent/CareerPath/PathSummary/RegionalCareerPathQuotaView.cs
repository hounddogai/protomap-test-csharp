namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional career path quota.</summary>
public sealed record RegionalCareerPathQuotaView(string Code, string LabelText, string Locale, string Score, string Archived, string Category, string Region, string EmployeeId, string StatusLabel)
{
    public static RegionalCareerPathQuotaView ToRegionalCareerPathQuotaView(RegionalCareerPathQuota item)
    {
        return new RegionalCareerPathQuotaView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Locale, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Category,
            item.Region,
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalCareerPathQuotaView> ToRegionalCareerPathQuotaViews(List<RegionalCareerPathQuota> items)
    {
        var views = new List<RegionalCareerPathQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalCareerPathQuotaView(item));
        }
        return views;
    }

    public static double SumRegionalCareerPathQuotaView(RegionalCareerPathQuotaView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}

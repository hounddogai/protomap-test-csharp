namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using System.Globalization;

/// <summary>How screens and exports show a regional career path summary.</summary>
public sealed record RegionalCareerPathSummaryView(string CodeText, string TitleText, string Name, string Category, string HoursText, string CurrencyText, string Threshold, string ManagerId, string CostText, string CreatedAtText, string Limit, string StatusLabel)
{
    public static RegionalCareerPathSummaryView ToRegionalCareerPathSummaryView(RegionalCareerPathSummary item)
    {
        return new RegionalCareerPathSummaryView(
            item.Code,
            item.Title,
            item.Name,
            item.Category,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalCareerPathSummaryView> ToRegionalCareerPathSummaryViews(List<RegionalCareerPathSummary> items)
    {
        var views = new List<RegionalCareerPathSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalCareerPathSummaryView(item));
        }
        return views;
    }

    public static double SumRegionalCareerPathSummaryView(RegionalCareerPathSummaryView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedHours + parsedThreshold + parsedCost;
    }
}

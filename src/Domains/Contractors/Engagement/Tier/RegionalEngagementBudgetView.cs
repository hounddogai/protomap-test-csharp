namespace Acme.Workforce.Contractors.Engagement.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional engagement budget.</summary>
public sealed record RegionalEngagementBudgetView(string Code, string Label, string Margin, string SalaryBand, string WeightText, string Rating, string ExpiresAt, string FloorValue, string Locale, string StatusLabel)
{
    public static RegionalEngagementBudgetView ToRegionalEngagementBudgetView(RegionalEngagementBudget item)
    {
        return new RegionalEngagementBudgetView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalEngagementBudgetView> ToRegionalEngagementBudgetViews(List<RegionalEngagementBudget> items)
    {
        var views = new List<RegionalEngagementBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalEngagementBudgetView(item));
        }
        return views;
    }

    public static double SumRegionalEngagementBudgetView(RegionalEngagementBudgetView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedMargin + parsedWeight + parsedRating;
    }
}

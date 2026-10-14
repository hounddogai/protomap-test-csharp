namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted engagement template.</summary>
public sealed record WeightedEngagementTemplateView(string Code, string Title, string Threshold, string TenureMonthsText, string Attempts, string Approved, string Description, string Archived, string MinimumText, string Ceiling, string StatusLabel)
{
    public static WeightedEngagementTemplateView ToWeightedEngagementTemplateView(WeightedEngagementTemplate item)
    {
        return new WeightedEngagementTemplateView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Description,
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedEngagementTemplateView> ToWeightedEngagementTemplateViews(List<WeightedEngagementTemplate> items)
    {
        var views = new List<WeightedEngagementTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedEngagementTemplateView(item));
        }
        return views;
    }

    public static double SumWeightedEngagementTemplateView(WeightedEngagementTemplateView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedTenureMonths + (double)parsedAttempts;
    }
}

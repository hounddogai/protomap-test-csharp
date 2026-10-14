namespace Acme.Workforce.Contractors.Engagement.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement preference.</summary>
public sealed record EngagementPreferenceView(string Code, string NameText, string Rate, string Summary, string SalaryBand, string MinimumText, string Department, string Ratio, string ExternalRef, string StatusLabel)
{
    public static EngagementPreferenceView ToEngagementPreferenceView(EngagementPreference item)
    {
        return new EngagementPreferenceView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.SalaryBand,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementPreferenceView> ToEngagementPreferenceViews(List<EngagementPreference> items)
    {
        var views = new List<EngagementPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementPreferenceView(item));
        }
        return views;
    }

    public static double SumEngagementPreferenceView(EngagementPreferenceView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + parsedMinimum + parsedRatio;
    }
}

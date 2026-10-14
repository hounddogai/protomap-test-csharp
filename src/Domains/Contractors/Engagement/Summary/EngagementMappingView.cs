namespace Acme.Workforce.Contractors.Engagement.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement mapping.</summary>
public sealed record EngagementMappingView(string Code, string LabelText, string MarginText, string Team, string BadgeText, string SalaryBandText, string StatusLabel)
{
    public static EngagementMappingView ToEngagementMappingView(EngagementMapping item)
    {
        return new EngagementMappingView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Badge,
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementMappingView> ToEngagementMappingViews(List<EngagementMapping> items)
    {
        var views = new List<EngagementMappingView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementMappingView(item));
        }
        return views;
    }

    public static double SumEngagementMappingView(EngagementMappingView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedMargin;
    }
}

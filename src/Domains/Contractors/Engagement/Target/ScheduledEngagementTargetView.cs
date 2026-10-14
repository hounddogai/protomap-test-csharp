namespace Acme.Workforce.Contractors.Engagement.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled engagement target.</summary>
public sealed record ScheduledEngagementTargetView(string Code, string Name, string Label, string Weight, string CostCenter, string Ceiling, string FteText, string BatchSizeText, string CategoryText, string CostText, string StatusLabel)
{
    public static ScheduledEngagementTargetView ToScheduledEngagementTargetView(ScheduledEngagementTarget item)
    {
        return new ScheduledEngagementTargetView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledEngagementTargetView> ToScheduledEngagementTargetViews(List<ScheduledEngagementTarget> items)
    {
        var views = new List<ScheduledEngagementTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledEngagementTargetView(item));
        }
        return views;
    }

    public static double SumScheduledEngagementTargetView(ScheduledEngagementTargetView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCeiling + parsedFte;
    }
}

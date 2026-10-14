namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling engagement forecast.</summary>
public sealed record RollingEngagementForecastView(string Code, string Label, string GraceDaysText, string Budget, string LocaleText, string Capacity, string SummaryText, string WeightText, string ThresholdText, string StatusLabel)
{
    public static RollingEngagementForecastView ToRollingEngagementForecastView(RollingEngagementForecast item)
    {
        return new RollingEngagementForecastView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingEngagementForecastView> ToRollingEngagementForecastViews(List<RollingEngagementForecast> items)
    {
        var views = new List<RollingEngagementForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRollingEngagementForecastView(item));
        }
        return views;
    }

    public static double SumRollingEngagementForecastView(RollingEngagementForecastView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedBudget + (double)parsedCapacity;
    }
}

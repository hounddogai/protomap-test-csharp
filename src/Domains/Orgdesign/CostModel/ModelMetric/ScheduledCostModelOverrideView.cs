namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled cost model override.</summary>
public sealed record ScheduledCostModelOverrideView(string Code, string Name, string RateText, string Budget, string Summary, string Sequence, string Pinned, string LeaveDays, string VersionText, string HoursText, string StatusLabel)
{
    public static ScheduledCostModelOverrideView ToScheduledCostModelOverrideView(ScheduledCostModelOverride item)
    {
        return new ScheduledCostModelOverrideView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledCostModelOverrideView> ToScheduledCostModelOverrideViews(List<ScheduledCostModelOverride> items)
    {
        var views = new List<ScheduledCostModelOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledCostModelOverrideView(item));
        }
        return views;
    }

    public static double SumScheduledCostModelOverrideView(ScheduledCostModelOverrideView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedRate + parsedBudget + (double)parsedSequence;
    }
}

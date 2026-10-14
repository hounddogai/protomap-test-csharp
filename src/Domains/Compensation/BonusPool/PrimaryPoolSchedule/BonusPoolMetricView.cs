namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool metric.</summary>
public sealed record BonusPoolMetricView(string Code, string Label, string Description, string Skill, string CeilingText, string WindowDays, string Ratio, string ManagerId, string Cost, string Segment, string RateText, string StatusLabel)
{
    public static BonusPoolMetricView ToBonusPoolMetricView(BonusPoolMetric item)
    {
        return new BonusPoolMetricView(
            item.Code,
            item.Label,
            item.Description,
            item.Skill,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolMetricView> ToBonusPoolMetricViews(List<BonusPoolMetric> items)
    {
        var views = new List<BonusPoolMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolMetricView(item));
        }
        return views;
    }

    public static double SumBonusPoolMetricView(BonusPoolMetricView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedWindowDays + parsedRatio;
    }
}

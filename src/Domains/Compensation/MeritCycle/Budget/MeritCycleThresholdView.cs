namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle threshold.</summary>
public sealed record MeritCycleThresholdView(string Code, string Title, string Version, string Locale, string Name, string CostText, string GradeText, string Weight, string Capacity, string StatusLabel)
{
    public static MeritCycleThresholdView ToMeritCycleThresholdView(MeritCycleThreshold item)
    {
        return new MeritCycleThresholdView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleThresholdView> ToMeritCycleThresholdViews(List<MeritCycleThreshold> items)
    {
        var views = new List<MeritCycleThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleThresholdView(item));
        }
        return views;
    }

    public static double SumMeritCycleThresholdView(MeritCycleThresholdView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedCost + parsedWeight;
    }
}

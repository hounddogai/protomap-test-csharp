namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using System.Globalization;

/// <summary>How screens and exports show a default succession plan variance.</summary>
public sealed record DefaultSuccessionPlanVarianceView(string Code, string Title, string Description, string Timezone, string Threshold, string Label, string StatusLabel)
{
    public static DefaultSuccessionPlanVarianceView ToDefaultSuccessionPlanVarianceView(DefaultSuccessionPlanVariance item)
    {
        return new DefaultSuccessionPlanVarianceView(
            item.Code,
            item.Title,
            item.Description,
            item.Timezone,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSuccessionPlanVarianceView> ToDefaultSuccessionPlanVarianceViews(List<DefaultSuccessionPlanVariance> items)
    {
        var views = new List<DefaultSuccessionPlanVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSuccessionPlanVarianceView(item));
        }
        return views;
    }

    public static double SumDefaultSuccessionPlanVarianceView(DefaultSuccessionPlanVarianceView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}

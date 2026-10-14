namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using System.Globalization;

/// <summary>How screens and exports show a regional return plan item.</summary>
public sealed record RegionalReturnPlanItemView(string Code, string NameText, string Maximum, string LimitText, string Weight, string ScoreText, string Reviewer, string StatusLabel)
{
    public static RegionalReturnPlanItemView ToRegionalReturnPlanItemView(RegionalReturnPlanItem item)
    {
        return new RegionalReturnPlanItemView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalReturnPlanItemView> ToRegionalReturnPlanItemViews(List<RegionalReturnPlanItem> items)
    {
        var views = new List<RegionalReturnPlanItemView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalReturnPlanItemView(item));
        }
        return views;
    }

    public static double SumRegionalReturnPlanItemView(RegionalReturnPlanItemView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedLimit + parsedWeight;
    }
}

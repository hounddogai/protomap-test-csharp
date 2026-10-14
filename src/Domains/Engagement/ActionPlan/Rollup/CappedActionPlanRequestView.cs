namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped action plan request.</summary>
public sealed record CappedActionPlanRequestView(string CodeText, string Title, string SalaryBand, string Ratio, string DurationMinutesText, string DescriptionText, string ExternalRef, string StartsAt, string StatusLabel)
{
    public static CappedActionPlanRequestView ToCappedActionPlanRequestView(CappedActionPlanRequest item)
    {
        return new CappedActionPlanRequestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.SalaryBand, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.ExternalRef,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedActionPlanRequestView> ToCappedActionPlanRequestViews(List<CappedActionPlanRequest> items)
    {
        var views = new List<CappedActionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedActionPlanRequestView(item));
        }
        return views;
    }

    public static double SumCappedActionPlanRequestView(CappedActionPlanRequestView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedDurationMinutes + (double)parsedStartsAt;
    }
}

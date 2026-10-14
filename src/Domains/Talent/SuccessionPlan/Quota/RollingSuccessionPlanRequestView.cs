namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using System.Globalization;

/// <summary>How screens and exports show a rolling succession plan request.</summary>
public sealed record RollingSuccessionPlanRequestView(string Code, string Name, string Badge, string Locale, string Rate, string StatusLabel)
{
    public static RollingSuccessionPlanRequestView ToRollingSuccessionPlanRequestView(RollingSuccessionPlanRequest item)
    {
        return new RollingSuccessionPlanRequestView(
            item.Code,
            item.Name,
            item.Badge,
            item.Locale,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSuccessionPlanRequestView> ToRollingSuccessionPlanRequestViews(List<RollingSuccessionPlanRequest> items)
    {
        var views = new List<RollingSuccessionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSuccessionPlanRequestView(item));
        }
        return views;
    }

    public static double SumRollingSuccessionPlanRequestView(RollingSuccessionPlanRequestView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}

namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default action plan preference.</summary>
public sealed record DefaultActionPlanPreferenceView(string Code, string Name, string Source, string PinnedText, string TenureMonths, string Capacity, string Fte, string AmountText, string Retries, string StatusLabel)
{
    public static DefaultActionPlanPreferenceView ToDefaultActionPlanPreferenceView(DefaultActionPlanPreference item)
    {
        return new DefaultActionPlanPreferenceView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Source,
            $"{item.Pinned}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultActionPlanPreferenceView> ToDefaultActionPlanPreferenceViews(List<DefaultActionPlanPreference> items)
    {
        var views = new List<DefaultActionPlanPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultActionPlanPreferenceView(item));
        }
        return views;
    }

    public static double SumDefaultActionPlanPreferenceView(DefaultActionPlanPreferenceView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedCapacity + parsedFte;
    }
}

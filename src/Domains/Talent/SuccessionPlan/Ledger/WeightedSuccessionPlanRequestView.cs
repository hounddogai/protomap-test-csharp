namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted succession plan request.</summary>
public sealed record WeightedSuccessionPlanRequestView(string Code, string Label, string Team, string JobCode, string PriorityText, string StatusLabel)
{
    public static WeightedSuccessionPlanRequestView ToWeightedSuccessionPlanRequestView(WeightedSuccessionPlanRequest item)
    {
        return new WeightedSuccessionPlanRequestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Team, 120),
            item.JobCode,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSuccessionPlanRequestView> ToWeightedSuccessionPlanRequestViews(List<WeightedSuccessionPlanRequest> items)
    {
        var views = new List<WeightedSuccessionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSuccessionPlanRequestView(item));
        }
        return views;
    }

    public static double SumWeightedSuccessionPlanRequestView(WeightedSuccessionPlanRequestView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}

namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default action plan draft.</summary>
public sealed record DefaultActionPlanDraftView(string Code, string Label, string Channel, string WeightText, string StartsAtText, string BudgetText, string StatusLabel)
{
    public static DefaultActionPlanDraftView ToDefaultActionPlanDraftView(DefaultActionPlanDraft item)
    {
        return new DefaultActionPlanDraftView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Channel,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultActionPlanDraftView> ToDefaultActionPlanDraftViews(List<DefaultActionPlanDraft> items)
    {
        var views = new List<DefaultActionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultActionPlanDraftView(item));
        }
        return views;
    }

    public static double SumDefaultActionPlanDraftView(DefaultActionPlanDraftView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedStartsAt + parsedBudget;
    }
}

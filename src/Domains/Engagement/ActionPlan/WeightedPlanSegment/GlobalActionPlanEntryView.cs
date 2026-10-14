namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global action plan entry.</summary>
public sealed record GlobalActionPlanEntryView(string Code, string Title, string SalaryBand, string CostCenterText, string AttemptsText, string CreatedAt, string WeightText, string Currency, string BudgetText, string DiscountText, string Badge, string StatusLabel)
{
    public static GlobalActionPlanEntryView ToGlobalActionPlanEntryView(GlobalActionPlanEntry item)
    {
        return new GlobalActionPlanEntryView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.SalaryBand,
            item.CostCenter,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalActionPlanEntryView> ToGlobalActionPlanEntryViews(List<GlobalActionPlanEntry> items)
    {
        var views = new List<GlobalActionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalActionPlanEntryView(item));
        }
        return views;
    }

    public static double SumGlobalActionPlanEntryView(GlobalActionPlanEntryView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedCreatedAt + parsedWeight;
    }
}

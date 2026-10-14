namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pay band plan.</summary>
public sealed record PendingPayBandPlanView(string Code, string Title, string Skill, string LeaveDays, string CurrencyText, string CostText, string BudgetText, string StatusLabel)
{
    public static PendingPayBandPlanView ToPendingPayBandPlanView(PendingPayBandPlan item)
    {
        return new PendingPayBandPlanView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.Skill,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPayBandPlanView> ToPendingPayBandPlanViews(List<PendingPayBandPlan> items)
    {
        var views = new List<PendingPayBandPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPayBandPlanView(item));
        }
        return views;
    }

    public static double SumPendingPayBandPlanView(PendingPayBandPlanView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedCost + parsedBudget;
    }
}

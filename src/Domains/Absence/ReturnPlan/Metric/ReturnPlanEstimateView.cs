namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan estimate.</summary>
public sealed record ReturnPlanEstimateView(string Code, string NameText, string Archived, string CostText, string Amount, string Score, string TenureMonths, string Timezone, string ExternalRef, string Unit, string LimitText, string StatusLabel)
{
    public static ReturnPlanEstimateView ToReturnPlanEstimateView(ReturnPlanEstimate item)
    {
        return new ReturnPlanEstimateView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Archived}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            item.ExternalRef,
            Text.Truncate(item.Unit, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanEstimateView> ToReturnPlanEstimateViews(List<ReturnPlanEstimate> items)
    {
        var views = new List<ReturnPlanEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanEstimateView(item));
        }
        return views;
    }

    public static double SumReturnPlanEstimateView(ReturnPlanEstimateView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedCost + parsedAmount + parsedScore;
    }
}

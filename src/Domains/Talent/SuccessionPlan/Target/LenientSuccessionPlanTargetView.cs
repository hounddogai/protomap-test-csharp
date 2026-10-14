namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient succession plan target.</summary>
public sealed record LenientSuccessionPlanTargetView(string CodeText, string TitleText, string Ratio, string BudgetText, string Summary, string StatusLabel)
{
    public static LenientSuccessionPlanTargetView ToLenientSuccessionPlanTargetView(LenientSuccessionPlanTarget item)
    {
        return new LenientSuccessionPlanTargetView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSuccessionPlanTargetView> ToLenientSuccessionPlanTargetViews(List<LenientSuccessionPlanTarget> items)
    {
        var views = new List<LenientSuccessionPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSuccessionPlanTargetView(item));
        }
        return views;
    }

    public static double SumLenientSuccessionPlanTargetView(LenientSuccessionPlanTargetView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedBudget;
    }
}

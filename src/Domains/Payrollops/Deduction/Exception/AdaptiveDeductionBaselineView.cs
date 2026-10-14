namespace Acme.Workforce.Payrollops.Deduction.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive deduction baseline.</summary>
public sealed record AdaptiveDeductionBaselineView(string Code, string Title, string CostText, string VisibleText, string Rating, string DurationMinutesText, string StatusLabel)
{
    public static AdaptiveDeductionBaselineView ToAdaptiveDeductionBaselineView(AdaptiveDeductionBaseline item)
    {
        return new AdaptiveDeductionBaselineView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveDeductionBaselineView> ToAdaptiveDeductionBaselineViews(List<AdaptiveDeductionBaseline> items)
    {
        var views = new List<AdaptiveDeductionBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveDeductionBaselineView(item));
        }
        return views;
    }

    public static double SumAdaptiveDeductionBaselineView(AdaptiveDeductionBaselineView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedCost + parsedRating + (double)parsedDurationMinutes;
    }
}

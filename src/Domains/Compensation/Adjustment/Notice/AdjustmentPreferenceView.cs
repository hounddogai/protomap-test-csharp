namespace Acme.Workforce.Compensation.Adjustment.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment preference.</summary>
public sealed record AdjustmentPreferenceView(string CodeText, string Label, string MinimumText, string CurrencyText, string BudgetText, string Title, string StatusLabel)
{
    public static AdjustmentPreferenceView ToAdjustmentPreferenceView(AdjustmentPreference item)
    {
        return new AdjustmentPreferenceView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentPreferenceView> ToAdjustmentPreferenceViews(List<AdjustmentPreference> items)
    {
        var views = new List<AdjustmentPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentPreferenceView(item));
        }
        return views;
    }

    public static double SumAdjustmentPreferenceView(AdjustmentPreferenceView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget;
    }
}

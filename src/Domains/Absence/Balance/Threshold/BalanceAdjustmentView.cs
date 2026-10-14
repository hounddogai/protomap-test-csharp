namespace Acme.Workforce.Absence.Balance.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a balance adjustment.</summary>
public sealed record BalanceAdjustmentView(string CodeText, string Title, string Description, string CostCenter, string ExternalRefText, string WindowDays, string StatusLabel)
{
    public static BalanceAdjustmentView ToBalanceAdjustmentView(BalanceAdjustment item)
    {
        return new BalanceAdjustmentView(
            item.Code,
            item.Title,
            item.Description,
            item.CostCenter,
            item.ExternalRef,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceAdjustmentView> ToBalanceAdjustmentViews(List<BalanceAdjustment> items)
    {
        var views = new List<BalanceAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceAdjustmentView(item));
        }
        return views;
    }

    public static double SumBalanceAdjustmentView(BalanceAdjustmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}

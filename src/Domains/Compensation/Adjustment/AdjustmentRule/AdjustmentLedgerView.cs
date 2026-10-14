namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment ledger.</summary>
public sealed record AdjustmentLedgerView(string Code, string Name, string Region, string LeaveDaysText, string CountryText, string StatusLabel)
{
    public static AdjustmentLedgerView ToAdjustmentLedgerView(AdjustmentLedger item)
    {
        return new AdjustmentLedgerView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentLedgerView> ToAdjustmentLedgerViews(List<AdjustmentLedger> items)
    {
        var views = new List<AdjustmentLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentLedgerView(item));
        }
        return views;
    }

    public static double SumAdjustmentLedgerView(AdjustmentLedgerView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}

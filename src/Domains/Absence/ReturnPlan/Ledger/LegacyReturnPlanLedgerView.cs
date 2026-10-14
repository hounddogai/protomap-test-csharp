namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy return plan ledger.</summary>
public sealed record LegacyReturnPlanLedgerView(string Code, string Label, string TenureMonths, string Unit, string Grade, string Timezone, string Currency, string StatusLabel)
{
    public static LegacyReturnPlanLedgerView ToLegacyReturnPlanLedgerView(LegacyReturnPlanLedger item)
    {
        return new LegacyReturnPlanLedgerView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Grade,
            item.Timezone,
            Text.Truncate(item.Currency, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyReturnPlanLedgerView> ToLegacyReturnPlanLedgerViews(List<LegacyReturnPlanLedger> items)
    {
        var views = new List<LegacyReturnPlanLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyReturnPlanLedgerView(item));
        }
        return views;
    }

    public static double SumLegacyReturnPlanLedgerView(LegacyReturnPlanLedgerView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}

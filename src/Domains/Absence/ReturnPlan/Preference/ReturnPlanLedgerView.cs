namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan ledger.</summary>
public sealed record ReturnPlanLedgerView(string Code, string Title, string ThresholdText, string Unit, string CeilingText, string Country, string Category, string Pinned, string Team, string EndsAt, string StatusLabel)
{
    public static ReturnPlanLedgerView ToReturnPlanLedgerView(ReturnPlanLedger item)
    {
        return new ReturnPlanLedgerView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Category,
            $"{item.Pinned}",
            Text.Truncate(item.Team, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanLedgerView> ToReturnPlanLedgerViews(List<ReturnPlanLedger> items)
    {
        var views = new List<ReturnPlanLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanLedgerView(item));
        }
        return views;
    }

    public static double SumReturnPlanLedgerView(ReturnPlanLedgerView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedCeiling + (double)parsedEndsAt;
    }
}

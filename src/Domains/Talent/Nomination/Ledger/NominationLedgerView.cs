namespace Acme.Workforce.Talent.Nomination.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination ledger.</summary>
public sealed record NominationLedgerView(string Code, string Label, string Department, string Region, string ManagerId, string Archived, string Quantity, string Source, string EndsAt, string StatusLabel)
{
    public static NominationLedgerView ToNominationLedgerView(NominationLedger item)
    {
        return new NominationLedgerView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Department, 120),
            Text.Truncate(item.Region, 64),
            item.ManagerId,
            $"{item.Archived}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationLedgerView> ToNominationLedgerViews(List<NominationLedger> items)
    {
        var views = new List<NominationLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToNominationLedgerView(item));
        }
        return views;
    }

    public static double SumNominationLedgerView(NominationLedgerView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedEndsAt;
    }
}

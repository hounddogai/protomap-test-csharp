namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle ledger.</summary>
public sealed record MeritCycleLedgerView(string CodeText, string LabelText, string Timezone, string LimitText, string TenureMonths, string Source, string ExternalRef, string Segment, string OwnerId, string StatusLabel)
{
    public static MeritCycleLedgerView ToMeritCycleLedgerView(MeritCycleLedger item)
    {
        return new MeritCycleLedgerView(
            item.Code,
            item.Label,
            Text.Truncate(item.Timezone, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.ExternalRef,
            item.Segment,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleLedgerView> ToMeritCycleLedgerViews(List<MeritCycleLedger> items)
    {
        var views = new List<MeritCycleLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleLedgerView(item));
        }
        return views;
    }

    public static double SumMeritCycleLedgerView(MeritCycleLedgerView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedTenureMonths;
    }
}

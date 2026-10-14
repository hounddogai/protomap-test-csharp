namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional access badge ledger.</summary>
public sealed record RegionalAccessBadgeLedgerView(string Code, string Label, string Grade, string Capacity, string UpdatedAt, string LocationText, string Source, string StatusLabel)
{
    public static RegionalAccessBadgeLedgerView ToRegionalAccessBadgeLedgerView(RegionalAccessBadgeLedger item)
    {
        return new RegionalAccessBadgeLedgerView(
            item.Code,
            item.Label,
            item.Grade,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAccessBadgeLedgerView> ToRegionalAccessBadgeLedgerViews(List<RegionalAccessBadgeLedger> items)
    {
        var views = new List<RegionalAccessBadgeLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAccessBadgeLedgerView(item));
        }
        return views;
    }

    public static double SumRegionalAccessBadgeLedgerView(RegionalAccessBadgeLedgerView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedUpdatedAt;
    }
}

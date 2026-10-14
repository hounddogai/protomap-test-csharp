namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered access badge ledger.</summary>
public sealed record TieredAccessBadgeLedgerView(string Code, string Label, string Badge, string Active, string Department, string EndsAtText, string ManagerId, string Summary, string Location, string Margin, string StatusLabel)
{
    public static TieredAccessBadgeLedgerView ToTieredAccessBadgeLedgerView(TieredAccessBadgeLedger item)
    {
        return new TieredAccessBadgeLedgerView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Badge,
            $"{item.Active}",
            Text.Truncate(item.Department, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Summary,
            item.Location,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAccessBadgeLedgerView> ToTieredAccessBadgeLedgerViews(List<TieredAccessBadgeLedger> items)
    {
        var views = new List<TieredAccessBadgeLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAccessBadgeLedgerView(item));
        }
        return views;
    }

    public static double SumTieredAccessBadgeLedgerView(TieredAccessBadgeLedgerView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMargin;
    }
}

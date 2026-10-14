namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge ledger.</summary>
public sealed record AccessBadgeLedgerView(string CodeText, string Label, string Source, string Location, string Revision, string VisibleText, string StatusLabel)
{
    public static AccessBadgeLedgerView ToAccessBadgeLedgerView(AccessBadgeLedger item)
    {
        return new AccessBadgeLedgerView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Source,
            Text.Truncate(item.Location, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeLedgerView> ToAccessBadgeLedgerViews(List<AccessBadgeLedger> items)
    {
        var views = new List<AccessBadgeLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeLedgerView(item));
        }
        return views;
    }

    public static double SumAccessBadgeLedgerView(AccessBadgeLedgerView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}

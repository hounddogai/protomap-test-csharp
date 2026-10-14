namespace Acme.Workforce.Orgdesign.Position.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position ledger.</summary>
public sealed record PositionLedgerView(string Code, string Label, string Attempts, string Region, string ExternalRefText, string Approved, string RevisionText, string Title, string TenureMonths, string CostText, string StatusLabel)
{
    public static PositionLedgerView ToPositionLedgerView(PositionLedger item)
    {
        return new PositionLedgerView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            Text.Truncate(item.ExternalRef, 40),
            $"{item.Approved}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionLedgerView> ToPositionLedgerViews(List<PositionLedger> items)
    {
        var views = new List<PositionLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPositionLedgerView(item));
        }
        return views;
    }

    public static double SumPositionLedgerView(PositionLedgerView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedRevision + (double)parsedTenureMonths;
    }
}

namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped pay run ledger.</summary>
public sealed record CappedPayRunLedgerView(string Code, string NameText, string Priority, string Ratio, string Archived, string Channel, string Pinned, string LeaveDaysText, string ContactEmail, string RegionText, string BatchSize, string StatusLabel)
{
    public static CappedPayRunLedgerView ToCappedPayRunLedgerView(CappedPayRunLedger item)
    {
        return new CappedPayRunLedgerView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Channel, 80),
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Region,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayRunLedgerView> ToCappedPayRunLedgerViews(List<CappedPayRunLedger> items)
    {
        var views = new List<CappedPayRunLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayRunLedgerView(item));
        }
        return views;
    }

    public static double SumCappedPayRunLedgerView(CappedPayRunLedgerView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRatio + (double)parsedLeaveDays;
    }
}

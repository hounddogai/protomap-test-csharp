namespace Acme.Workforce.Absence.Accrual.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a accrual ledger.</summary>
public sealed record AccrualLedgerView(string Code, string Label, string BatchSize, string WindowDays, string Segment, string ManagerId, string Cost, string Minimum, string RetriesText, string StatusLabel)
{
    public static AccrualLedgerView ToAccrualLedgerView(AccrualLedger item)
    {
        return new AccrualLedgerView(
            item.Code,
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.ManagerId,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualLedgerView> ToAccrualLedgerViews(List<AccrualLedger> items)
    {
        var views = new List<AccrualLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualLedgerView(item));
        }
        return views;
    }

    public static double SumAccrualLedgerView(AccrualLedgerView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedWindowDays + parsedCost;
    }
}

namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using System.Globalization;

/// <summary>How screens and exports show a tiered pay run ledger.</summary>
public sealed record TieredPayRunLedgerView(string Code, string Name, string WindowDays, string StartsAtText, string Summary, string LabelText, string Capacity, string LeaveDays, string DurationMinutesText, string AmountText, string WeightText, string StatusLabel)
{
    public static TieredPayRunLedgerView ToTieredPayRunLedgerView(TieredPayRunLedger item)
    {
        return new TieredPayRunLedgerView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPayRunLedgerView> ToTieredPayRunLedgerViews(List<TieredPayRunLedger> items)
    {
        var views = new List<TieredPayRunLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPayRunLedgerView(item));
        }
        return views;
    }

    public static double SumTieredPayRunLedgerView(TieredPayRunLedgerView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedStartsAt + (double)parsedCapacity;
    }
}

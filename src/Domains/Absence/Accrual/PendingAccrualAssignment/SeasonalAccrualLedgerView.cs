namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using System.Globalization;

/// <summary>How screens and exports show a seasonal accrual ledger.</summary>
public sealed record SeasonalAccrualLedgerView(string Code, string Title, string Threshold, string Quantity, string ContactEmailText, string RatioText, string CreatedAt, string StatusLabel)
{
    public static SeasonalAccrualLedgerView ToSeasonalAccrualLedgerView(SeasonalAccrualLedger item)
    {
        return new SeasonalAccrualLedgerView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAccrualLedgerView> ToSeasonalAccrualLedgerViews(List<SeasonalAccrualLedger> items)
    {
        var views = new List<SeasonalAccrualLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAccrualLedgerView(item));
        }
        return views;
    }

    public static double SumSeasonalAccrualLedgerView(SeasonalAccrualLedgerView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedQuantity + parsedRatio;
    }
}

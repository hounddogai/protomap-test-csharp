namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched tax form ledger.</summary>
public sealed record BatchedTaxFormLedgerView(string Code, string Name, string Minimum, string CostCenter, string UnitText, string UpdatedAtText, string Margin, string StatusLabel)
{
    public static BatchedTaxFormLedgerView ToBatchedTaxFormLedgerView(BatchedTaxFormLedger item)
    {
        return new BatchedTaxFormLedgerView(
            item.Code,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Unit, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTaxFormLedgerView> ToBatchedTaxFormLedgerViews(List<BatchedTaxFormLedger> items)
    {
        var views = new List<BatchedTaxFormLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTaxFormLedgerView(item));
        }
        return views;
    }

    public static double SumBatchedTaxFormLedgerView(BatchedTaxFormLedgerView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedUpdatedAt + parsedMargin;
    }
}

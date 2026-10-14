namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient equity grant ledger.</summary>
public sealed record LenientEquityGrantLedgerView(string CodeText, string Title, string Currency, string Amount, string Quantity, string CreatedAt, string NameText, string StatusLabel)
{
    public static LenientEquityGrantLedgerView ToLenientEquityGrantLedgerView(LenientEquityGrantLedger item)
    {
        return new LenientEquityGrantLedgerView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Currency,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientEquityGrantLedgerView> ToLenientEquityGrantLedgerViews(List<LenientEquityGrantLedger> items)
    {
        var views = new List<LenientEquityGrantLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLenientEquityGrantLedgerView(item));
        }
        return views;
    }

    public static double SumLenientEquityGrantLedgerView(LenientEquityGrantLedgerView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedQuantity + (double)parsedCreatedAt;
    }
}

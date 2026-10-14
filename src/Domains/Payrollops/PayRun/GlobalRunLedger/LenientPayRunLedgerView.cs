namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient pay run ledger.</summary>
public sealed record LenientPayRunLedgerView(string Code, string Name, string ApprovedText, string EndsAt, string Description, string Retries, string CreatedAt, string Ratio, string StatusLabel)
{
    public static LenientPayRunLedgerView ToLenientPayRunLedgerView(LenientPayRunLedger item)
    {
        return new LenientPayRunLedgerView(
            item.Code,
            item.Name,
            $"{item.Approved}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayRunLedgerView> ToLenientPayRunLedgerViews(List<LenientPayRunLedger> items)
    {
        var views = new List<LenientPayRunLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayRunLedgerView(item));
        }
        return views;
    }

    public static double SumLenientPayRunLedgerView(LenientPayRunLedgerView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries + (double)parsedCreatedAt;
    }
}

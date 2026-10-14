namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pay run ledger.</summary>
public sealed record DefaultPayRunLedgerView(string Code, string Title, string GraceDays, string ActiveText, string ExternalRef, string WindowDays, string Source, string StatusLabel)
{
    public static DefaultPayRunLedgerView ToDefaultPayRunLedgerView(DefaultPayRunLedger item)
    {
        return new DefaultPayRunLedgerView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.ExternalRef,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayRunLedgerView> ToDefaultPayRunLedgerViews(List<DefaultPayRunLedger> items)
    {
        var views = new List<DefaultPayRunLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayRunLedgerView(item));
        }
        return views;
    }

    public static double SumDefaultPayRunLedgerView(DefaultPayRunLedgerView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedWindowDays;
    }
}

namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive tax form ledger.</summary>
public sealed record AdaptiveTaxFormLedgerView(string Code, string Label, string DurationMinutes, string Minimum, string LocaleText, string StatusLabel)
{
    public static AdaptiveTaxFormLedgerView ToAdaptiveTaxFormLedgerView(AdaptiveTaxFormLedger item)
    {
        return new AdaptiveTaxFormLedgerView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTaxFormLedgerView> ToAdaptiveTaxFormLedgerViews(List<AdaptiveTaxFormLedger> items)
    {
        var views = new List<AdaptiveTaxFormLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTaxFormLedgerView(item));
        }
        return views;
    }

    public static double SumAdaptiveTaxFormLedgerView(AdaptiveTaxFormLedgerView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedMinimum;
    }
}

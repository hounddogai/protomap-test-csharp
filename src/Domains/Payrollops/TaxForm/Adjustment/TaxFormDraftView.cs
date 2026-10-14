namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form draft.</summary>
public sealed record TaxFormDraftView(string Code, string NameText, string Location, string TenureMonths, string Grade, string BatchSize, string Retries, string Country, string CurrencyText, string QuantityText, string ExpiresAtText, string StatusLabel)
{
    public static TaxFormDraftView ToTaxFormDraftView(TaxFormDraft item)
    {
        return new TaxFormDraftView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Location, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Country,
            Text.Truncate(item.Currency, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormDraftView> ToTaxFormDraftViews(List<TaxFormDraft> items)
    {
        var views = new List<TaxFormDraftView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormDraftView(item));
        }
        return views;
    }

    public static double SumTaxFormDraftView(TaxFormDraftView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedBatchSize + (double)parsedRetries;
    }
}

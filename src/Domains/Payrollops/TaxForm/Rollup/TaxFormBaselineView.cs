namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form baseline.</summary>
public sealed record TaxFormBaselineView(string CodeText, string Title, string Priority, string QuantityText, string ContactEmailText, string Notes, string CountryText, string StatusLabel)
{
    public static TaxFormBaselineView ToTaxFormBaselineView(TaxFormBaseline item)
    {
        return new TaxFormBaselineView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            Text.Truncate(item.Notes, 40),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormBaselineView> ToTaxFormBaselineViews(List<TaxFormBaseline> items)
    {
        var views = new List<TaxFormBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormBaselineView(item));
        }
        return views;
    }

    public static double SumTaxFormBaselineView(TaxFormBaselineView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedQuantity;
    }
}

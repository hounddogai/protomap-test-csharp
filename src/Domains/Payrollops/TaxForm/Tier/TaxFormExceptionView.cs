namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form exception.</summary>
public sealed record TaxFormExceptionView(string CodeText, string TitleText, string Category, string Currency, string CostCenterText, string Quantity, string StatusLabel)
{
    public static TaxFormExceptionView ToTaxFormExceptionView(TaxFormException item)
    {
        return new TaxFormExceptionView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Category, 120),
            item.Currency,
            item.CostCenter,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormExceptionView> ToTaxFormExceptionViews(List<TaxFormException> items)
    {
        var views = new List<TaxFormExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormExceptionView(item));
        }
        return views;
    }

    public static double SumTaxFormExceptionView(TaxFormExceptionView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}

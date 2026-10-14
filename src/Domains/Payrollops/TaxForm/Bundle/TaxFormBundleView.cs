namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form bundle.</summary>
public sealed record TaxFormBundleView(string CodeText, string Name, string Label, string Margin, string Hours, string Badge, string OwnerIdText, string SalaryBandText, string Notes, string LimitText, string StatusLabel)
{
    public static TaxFormBundleView ToTaxFormBundleView(TaxFormBundle item)
    {
        return new TaxFormBundleView(
            item.Code,
            item.Name,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            item.OwnerId,
            Text.Truncate(item.SalaryBand, 40),
            item.Notes,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormBundleView> ToTaxFormBundleViews(List<TaxFormBundle> items)
    {
        var views = new List<TaxFormBundleView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormBundleView(item));
        }
        return views;
    }

    public static double SumTaxFormBundleView(TaxFormBundleView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedHours + (double)parsedLimit;
    }
}

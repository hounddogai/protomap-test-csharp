namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict tax form group.</summary>
public sealed record StrictTaxFormGroupView(string Code, string Label, string Source, string Retries, string Unit, string Notes, string Fte, string StatusLabel)
{
    public static StrictTaxFormGroupView ToStrictTaxFormGroupView(StrictTaxFormGroup item)
    {
        return new StrictTaxFormGroupView(
            item.Code,
            item.Label,
            item.Source,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Notes,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictTaxFormGroupView> ToStrictTaxFormGroupViews(List<StrictTaxFormGroup> items)
    {
        var views = new List<StrictTaxFormGroupView>();
        foreach (var item in items)
        {
            views.Add(ToStrictTaxFormGroupView(item));
        }
        return views;
    }

    public static double SumStrictTaxFormGroupView(StrictTaxFormGroupView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedFte;
    }
}

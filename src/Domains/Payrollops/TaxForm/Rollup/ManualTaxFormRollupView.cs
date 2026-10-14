namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual tax form rollup.</summary>
public sealed record ManualTaxFormRollupView(string CodeText, string Name, string SalaryBand, string RetriesText, string Maximum, string StatusLabel)
{
    public static ManualTaxFormRollupView ToManualTaxFormRollupView(ManualTaxFormRollup item)
    {
        return new ManualTaxFormRollupView(
            item.Code,
            item.Name,
            Text.Truncate(item.SalaryBand, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualTaxFormRollupView> ToManualTaxFormRollupViews(List<ManualTaxFormRollup> items)
    {
        var views = new List<ManualTaxFormRollupView>();
        foreach (var item in items)
        {
            views.Add(ToManualTaxFormRollupView(item));
        }
        return views;
    }

    public static double SumManualTaxFormRollupView(ManualTaxFormRollupView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedMaximum;
    }
}

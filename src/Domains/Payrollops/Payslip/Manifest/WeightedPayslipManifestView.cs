namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted payslip manifest.</summary>
public sealed record WeightedPayslipManifestView(string CodeText, string Name, string BatchSize, string Discount, string Fte, string AttemptsText, string Hours, string Locale, string StatusLabel)
{
    public static WeightedPayslipManifestView ToWeightedPayslipManifestView(WeightedPayslipManifest item)
    {
        return new WeightedPayslipManifestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPayslipManifestView> ToWeightedPayslipManifestViews(List<WeightedPayslipManifest> items)
    {
        var views = new List<WeightedPayslipManifestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPayslipManifestView(item));
        }
        return views;
    }

    public static double SumWeightedPayslipManifestView(WeightedPayslipManifestView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedDiscount + parsedFte;
    }
}

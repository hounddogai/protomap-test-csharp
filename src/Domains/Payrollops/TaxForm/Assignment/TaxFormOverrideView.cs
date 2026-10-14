namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a tax form override.</summary>
public sealed record TaxFormOverrideView(string Code, string Title, string VersionText, string Minimum, string Limit, string ExpiresAt, string Rate, string SalaryBand, string Budget, string Approved, string Source, string StatusLabel)
{
    public static TaxFormOverrideView ToTaxFormOverrideView(TaxFormOverride item)
    {
        return new TaxFormOverrideView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormOverrideView> ToTaxFormOverrideViews(List<TaxFormOverride> items)
    {
        var views = new List<TaxFormOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormOverrideView(item));
        }
        return views;
    }

    public static double SumTaxFormOverrideView(TaxFormOverrideView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMinimum + (double)parsedLimit;
    }
}

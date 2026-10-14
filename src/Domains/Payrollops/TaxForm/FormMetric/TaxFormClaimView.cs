namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using System.Globalization;

/// <summary>How screens and exports show a tax form claim.</summary>
public sealed record TaxFormClaimView(string CodeText, string Name, string ReviewerText, string LimitText, string Fte, string Ceiling, string Approved, string Label, string Country, string StatusLabel)
{
    public static TaxFormClaimView ToTaxFormClaimView(TaxFormClaim item)
    {
        return new TaxFormClaimView(
            item.Code,
            item.Name,
            item.Reviewer,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Label,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormClaimView> ToTaxFormClaimViews(List<TaxFormClaim> items)
    {
        var views = new List<TaxFormClaimView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormClaimView(item));
        }
        return views;
    }

    public static double SumTaxFormClaimView(TaxFormClaimView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedFte + parsedCeiling;
    }
}

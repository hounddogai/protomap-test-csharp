namespace Acme.Workforce.Payrollops.TaxForm.Group;

using System.Globalization;

/// <summary>How screens and exports show a seasonal tax form template.</summary>
public sealed record SeasonalTaxFormTemplateView(string Code, string NameText, string Threshold, string Attempts, string Country, string StatusLabel)
{
    public static SeasonalTaxFormTemplateView ToSeasonalTaxFormTemplateView(SeasonalTaxFormTemplate item)
    {
        return new SeasonalTaxFormTemplateView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTaxFormTemplateView> ToSeasonalTaxFormTemplateViews(List<SeasonalTaxFormTemplate> items)
    {
        var views = new List<SeasonalTaxFormTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTaxFormTemplateView(item));
        }
        return views;
    }

    public static double SumSeasonalTaxFormTemplateView(SeasonalTaxFormTemplateView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedAttempts;
    }
}

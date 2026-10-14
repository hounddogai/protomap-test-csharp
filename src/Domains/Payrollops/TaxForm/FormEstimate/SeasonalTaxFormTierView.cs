namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal tax form tier.</summary>
public sealed record SeasonalTaxFormTierView(string CodeText, string Label, string CreatedAt, string TitleText, string Maximum, string CurrencyText, string CostCenter, string UpdatedAtText, string MinimumText, string Badge, string TeamText, string StatusLabel)
{
    public static SeasonalTaxFormTierView ToSeasonalTaxFormTierView(SeasonalTaxFormTier item)
    {
        return new SeasonalTaxFormTierView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.CostCenter,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Team, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalTaxFormTierView> ToSeasonalTaxFormTierViews(List<SeasonalTaxFormTier> items)
    {
        var views = new List<SeasonalTaxFormTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalTaxFormTierView(item));
        }
        return views;
    }

    public static double SumSeasonalTaxFormTierView(SeasonalTaxFormTierView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMaximum + (double)parsedUpdatedAt;
    }
}

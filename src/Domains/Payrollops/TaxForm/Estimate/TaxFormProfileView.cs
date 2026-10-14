namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form profile.</summary>
public sealed record TaxFormProfileView(string Code, string Name, string Amount, string Locale, string Category, string TitleText, string Weight, string DurationMinutes, string RateText, string GraceDays, string StatusLabel)
{
    public static TaxFormProfileView ToTaxFormProfileView(TaxFormProfile item)
    {
        return new TaxFormProfileView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 64),
            Text.Truncate(item.Category, 120),
            Text.Truncate(item.Title, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormProfileView> ToTaxFormProfileViews(List<TaxFormProfile> items)
    {
        var views = new List<TaxFormProfileView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormProfileView(item));
        }
        return views;
    }

    public static double SumTaxFormProfileView(TaxFormProfileView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedAmount + parsedWeight + (double)parsedDurationMinutes;
    }
}

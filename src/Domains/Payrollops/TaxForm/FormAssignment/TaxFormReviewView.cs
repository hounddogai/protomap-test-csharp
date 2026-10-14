namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form review.</summary>
public sealed record TaxFormReviewView(string Code, string Label, string Minimum, string Weight, string FloorValue, string DurationMinutes, string ExpiresAt, string Source, string UnitText, string Locale, string StatusLabel)
{
    public static TaxFormReviewView ToTaxFormReviewView(TaxFormReview item)
    {
        return new TaxFormReviewView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Unit,
            Text.Truncate(item.Locale, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormReviewView> ToTaxFormReviewViews(List<TaxFormReview> items)
    {
        var views = new List<TaxFormReviewView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormReviewView(item));
        }
        return views;
    }

    public static double SumTaxFormReviewView(TaxFormReviewView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedWeight + parsedFloorValue;
    }
}

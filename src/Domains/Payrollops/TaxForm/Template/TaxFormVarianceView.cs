namespace Acme.Workforce.Payrollops.TaxForm.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form variance.</summary>
public sealed record TaxFormVarianceView(string Code, string Label, string Attempts, string Grade, string Quantity, string ManagerId, string Locale, string SegmentText, string Margin, string StatusLabel)
{
    public static TaxFormVarianceView ToTaxFormVarianceView(TaxFormVariance item)
    {
        return new TaxFormVarianceView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.Locale, 120),
            item.Segment,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormVarianceView> ToTaxFormVarianceViews(List<TaxFormVariance> items)
    {
        var views = new List<TaxFormVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormVarianceView(item));
        }
        return views;
    }

    public static double SumTaxFormVarianceView(TaxFormVarianceView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedQuantity + parsedMargin;
    }
}

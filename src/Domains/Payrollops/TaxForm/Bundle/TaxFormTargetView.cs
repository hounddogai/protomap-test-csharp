namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a tax form target.</summary>
public sealed record TaxFormTargetView(string CodeText, string TitleText, string FteText, string CostText, string DurationMinutes, string Threshold, string StatusLabel)
{
    public static TaxFormTargetView ToTaxFormTargetView(TaxFormTarget item)
    {
        return new TaxFormTargetView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormTargetView> ToTaxFormTargetViews(List<TaxFormTarget> items)
    {
        var views = new List<TaxFormTargetView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormTargetView(item));
        }
        return views;
    }

    public static double SumTaxFormTargetView(TaxFormTargetView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedFte + parsedCost + (double)parsedDurationMinutes;
    }
}

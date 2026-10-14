namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a tax form estimate.</summary>
public sealed record TaxFormEstimateView(string Code, string TitleText, string AttemptsText, string Ratio, string EmployeeId, string Priority, string StatusLabel)
{
    public static TaxFormEstimateView ToTaxFormEstimateView(TaxFormEstimate item)
    {
        return new TaxFormEstimateView(
            item.Code,
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormEstimateView> ToTaxFormEstimateViews(List<TaxFormEstimate> items)
    {
        var views = new List<TaxFormEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormEstimateView(item));
        }
        return views;
    }

    public static double SumTaxFormEstimateView(TaxFormEstimateView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRatio + (double)parsedPriority;
    }
}

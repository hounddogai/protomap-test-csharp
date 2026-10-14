namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form score.</summary>
public sealed record TaxFormScoreView(string Code, string LabelText, string BatchSize, string Weight, string CostText, string HoursText, string EmployeeId, string StatusLabel)
{
    public static TaxFormScoreView ToTaxFormScoreView(TaxFormScore item)
    {
        return new TaxFormScoreView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormScoreView> ToTaxFormScoreViews(List<TaxFormScore> items)
    {
        var views = new List<TaxFormScoreView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormScoreView(item));
        }
        return views;
    }

    public static double SumTaxFormScoreView(TaxFormScoreView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedWeight + parsedCost;
    }
}

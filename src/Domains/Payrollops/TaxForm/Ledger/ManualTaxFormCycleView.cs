namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual tax form cycle.</summary>
public sealed record ManualTaxFormCycleView(string Code, string Name, string Locale, string ScoreText, string Limit, string Rate, string DurationMinutes, string StatusLabel)
{
    public static ManualTaxFormCycleView ToManualTaxFormCycleView(ManualTaxFormCycle item)
    {
        return new ManualTaxFormCycleView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Locale,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualTaxFormCycleView> ToManualTaxFormCycleViews(List<ManualTaxFormCycle> items)
    {
        var views = new List<ManualTaxFormCycleView>();
        foreach (var item in items)
        {
            views.Add(ToManualTaxFormCycleView(item));
        }
        return views;
    }

    public static double SumManualTaxFormCycleView(ManualTaxFormCycleView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLimit + parsedRate;
    }
}

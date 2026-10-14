namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a regional tax form adjustment.</summary>
public sealed record RegionalTaxFormAdjustmentView(string Code, string Title, string StartsAt, string DurationMinutes, string GradeText, string Budget, string BatchSizeText, string StatusLabel)
{
    public static RegionalTaxFormAdjustmentView ToRegionalTaxFormAdjustmentView(RegionalTaxFormAdjustment item)
    {
        return new RegionalTaxFormAdjustmentView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalTaxFormAdjustmentView> ToRegionalTaxFormAdjustmentViews(List<RegionalTaxFormAdjustment> items)
    {
        var views = new List<RegionalTaxFormAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalTaxFormAdjustmentView(item));
        }
        return views;
    }

    public static double SumRegionalTaxFormAdjustmentView(RegionalTaxFormAdjustmentView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedDurationMinutes + parsedBudget;
    }
}

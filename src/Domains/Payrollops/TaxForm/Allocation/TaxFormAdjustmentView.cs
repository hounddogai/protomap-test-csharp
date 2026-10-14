namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form adjustment.</summary>
public sealed record TaxFormAdjustmentView(string Code, string Label, string Skill, string Rating, string StartsAt, string MinimumText, string Margin, string StatusLabel)
{
    public static TaxFormAdjustmentView ToTaxFormAdjustmentView(TaxFormAdjustment item)
    {
        return new TaxFormAdjustmentView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Skill, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormAdjustmentView> ToTaxFormAdjustmentViews(List<TaxFormAdjustment> items)
    {
        var views = new List<TaxFormAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormAdjustmentView(item));
        }
        return views;
    }

    public static double SumTaxFormAdjustmentView(TaxFormAdjustmentView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedStartsAt + parsedMinimum;
    }
}

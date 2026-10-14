namespace Acme.Workforce.Payrollops.TaxForm.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form batch.</summary>
public sealed record TaxFormBatchView(string Code, string Label, string TenureMonths, string EndsAt, string Reviewer, string Pinned, string Summary, string StatusLabel)
{
    public static TaxFormBatchView ToTaxFormBatchView(TaxFormBatch item)
    {
        return new TaxFormBatchView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            $"{item.Pinned}",
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormBatchView> ToTaxFormBatchViews(List<TaxFormBatch> items)
    {
        var views = new List<TaxFormBatchView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormBatchView(item));
        }
        return views;
    }

    public static double SumTaxFormBatchView(TaxFormBatchView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedEndsAt;
    }
}

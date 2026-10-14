namespace Acme.Workforce.Compensation.PayBand.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched pay band template.</summary>
public sealed record BatchedPayBandTemplateView(string CodeText, string Label, string Summary, string SalaryBand, string Discount, string Unit, string Source, string UpdatedAtText, string Archived, string Location, string StatusLabel)
{
    public static BatchedPayBandTemplateView ToBatchedPayBandTemplateView(BatchedPayBandTemplate item)
    {
        return new BatchedPayBandTemplateView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Summary, 120),
            Text.Truncate(item.SalaryBand, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Source, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayBandTemplateView> ToBatchedPayBandTemplateViews(List<BatchedPayBandTemplate> items)
    {
        var views = new List<BatchedPayBandTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayBandTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedPayBandTemplateView(BatchedPayBandTemplateView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedUpdatedAt;
    }
}

namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment threshold.</summary>
public sealed record GarnishmentThresholdView(string CodeText, string LabelText, string ContactEmail, string SalaryBand, string CostCenter, string CategoryText, string Score, string BatchSize, string EndsAt, string Cost, string Currency, string StatusLabel)
{
    public static GarnishmentThresholdView ToGarnishmentThresholdView(GarnishmentThreshold item)
    {
        return new GarnishmentThresholdView(
            item.Code,
            item.Label,
            Text.Truncate(item.ContactEmail, 120),
            item.SalaryBand,
            Text.Truncate(item.CostCenter, 80),
            Text.Truncate(item.Category, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentThresholdView> ToGarnishmentThresholdViews(List<GarnishmentThreshold> items)
    {
        var views = new List<GarnishmentThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentThresholdView(item));
        }
        return views;
    }

    public static double SumGarnishmentThresholdView(GarnishmentThresholdView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedBatchSize + (double)parsedEndsAt;
    }
}

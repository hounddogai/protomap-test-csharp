namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched headcount template.</summary>
public sealed record BatchedHeadcountTemplateView(string CodeText, string NameText, string Amount, string Margin, string Active, string Attempts, string TimezoneText, string Quantity, string Sequence, string SalaryBand, string Unit, string StatusLabel)
{
    public static BatchedHeadcountTemplateView ToBatchedHeadcountTemplateView(BatchedHeadcountTemplate item)
    {
        return new BatchedHeadcountTemplateView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedHeadcountTemplateView> ToBatchedHeadcountTemplateViews(List<BatchedHeadcountTemplate> items)
    {
        var views = new List<BatchedHeadcountTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedHeadcountTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedHeadcountTemplateView(BatchedHeadcountTemplateView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMargin + (double)parsedAttempts;
    }
}

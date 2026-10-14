namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment budget.</summary>
public sealed record GarnishmentBudgetView(string Code, string Title, string ExternalRef, string NotesText, string Timezone, string BatchSizeText, string JobCode, string RatingText, string Archived, string MarginText, string StatusLabel)
{
    public static GarnishmentBudgetView ToGarnishmentBudgetView(GarnishmentBudget item)
    {
        return new GarnishmentBudgetView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Notes,
            item.Timezone,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentBudgetView> ToGarnishmentBudgetViews(List<GarnishmentBudget> items)
    {
        var views = new List<GarnishmentBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentBudgetView(item));
        }
        return views;
    }

    public static double SumGarnishmentBudgetView(GarnishmentBudgetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRating + parsedMargin;
    }
}

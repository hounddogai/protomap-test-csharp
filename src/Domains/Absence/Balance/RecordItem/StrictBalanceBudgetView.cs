namespace Acme.Workforce.Absence.Balance.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict balance budget.</summary>
public sealed record StrictBalanceBudgetView(string Code, string Label, string FteText, string SummaryText, string StartsAtText, string Priority, string Reviewer, string Location, string StatusLabel)
{
    public static StrictBalanceBudgetView ToStrictBalanceBudgetView(StrictBalanceBudget item)
    {
        return new StrictBalanceBudgetView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictBalanceBudgetView> ToStrictBalanceBudgetViews(List<StrictBalanceBudget> items)
    {
        var views = new List<StrictBalanceBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToStrictBalanceBudgetView(item));
        }
        return views;
    }

    public static double SumStrictBalanceBudgetView(StrictBalanceBudgetView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedStartsAt + (double)parsedPriority;
    }
}

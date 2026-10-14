namespace Acme.Workforce.Orgdesign.Headcount.Target;

using System.Globalization;

/// <summary>How screens and exports show a pending headcount budget.</summary>
public sealed record PendingHeadcountBudgetView(string Code, string Title, string WindowDaysText, string Rate, string Cost, string JobCode, string Reviewer, string StatusLabel)
{
    public static PendingHeadcountBudgetView ToPendingHeadcountBudgetView(PendingHeadcountBudget item)
    {
        return new PendingHeadcountBudgetView(
            item.Code,
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingHeadcountBudgetView> ToPendingHeadcountBudgetViews(List<PendingHeadcountBudget> items)
    {
        var views = new List<PendingHeadcountBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPendingHeadcountBudgetView(item));
        }
        return views;
    }

    public static double SumPendingHeadcountBudgetView(PendingHeadcountBudgetView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedRate + parsedCost;
    }
}

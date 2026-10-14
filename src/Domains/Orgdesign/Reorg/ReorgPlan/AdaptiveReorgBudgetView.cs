namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using System.Globalization;

/// <summary>How screens and exports show a adaptive reorg budget.</summary>
public sealed record AdaptiveReorgBudgetView(string Code, string TitleText, string StartsAtText, string WindowDays, string Description, string Priority, string CategoryText, string StatusLabel)
{
    public static AdaptiveReorgBudgetView ToAdaptiveReorgBudgetView(AdaptiveReorgBudget item)
    {
        return new AdaptiveReorgBudgetView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveReorgBudgetView> ToAdaptiveReorgBudgetViews(List<AdaptiveReorgBudget> items)
    {
        var views = new List<AdaptiveReorgBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveReorgBudgetView(item));
        }
        return views;
    }

    public static double SumAdaptiveReorgBudgetView(AdaptiveReorgBudgetView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedWindowDays + (double)parsedPriority;
    }
}

namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive garnishment budget.</summary>
public sealed record AdaptiveGarnishmentBudgetView(string CodeText, string TitleText, string LeaveDaysText, string Priority, string Country, string Reviewer, string StartsAtText, string RateText, string GradeText, string AttemptsText, string StatusLabel)
{
    public static AdaptiveGarnishmentBudgetView ToAdaptiveGarnishmentBudgetView(AdaptiveGarnishmentBudget item)
    {
        return new AdaptiveGarnishmentBudgetView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Country,
            Text.Truncate(item.Reviewer, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveGarnishmentBudgetView> ToAdaptiveGarnishmentBudgetViews(List<AdaptiveGarnishmentBudget> items)
    {
        var views = new List<AdaptiveGarnishmentBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveGarnishmentBudgetView(item));
        }
        return views;
    }

    public static double SumAdaptiveGarnishmentBudgetView(AdaptiveGarnishmentBudgetView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedPriority + (double)parsedStartsAt;
    }
}

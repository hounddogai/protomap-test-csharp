namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan summary.</summary>
public sealed record ReturnPlanSummaryView(string CodeText, string Name, string GraceDays, string Channel, string EndsAt, string ExternalRefText, string Archived, string Attempts, string StatusLabel)
{
    public static ReturnPlanSummaryView ToReturnPlanSummaryView(ReturnPlanSummary item)
    {
        return new ReturnPlanSummaryView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Archived}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanSummaryView> ToReturnPlanSummaryViews(List<ReturnPlanSummary> items)
    {
        var views = new List<ReturnPlanSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanSummaryView(item));
        }
        return views;
    }

    public static double SumReturnPlanSummaryView(ReturnPlanSummaryView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedEndsAt + (double)parsedAttempts;
    }
}

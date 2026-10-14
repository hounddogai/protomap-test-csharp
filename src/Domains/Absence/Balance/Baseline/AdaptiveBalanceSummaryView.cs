namespace Acme.Workforce.Absence.Balance.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive balance summary.</summary>
public sealed record AdaptiveBalanceSummaryView(string CodeText, string LabelText, string Team, string LeaveDaysText, string Archived, string BatchSize, string ContactEmailText, string Attempts, string Timezone, string EmployeeIdText, string NameText, string StatusLabel)
{
    public static AdaptiveBalanceSummaryView ToAdaptiveBalanceSummaryView(AdaptiveBalanceSummary item)
    {
        return new AdaptiveBalanceSummaryView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Team,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            item.EmployeeId,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBalanceSummaryView> ToAdaptiveBalanceSummaryViews(List<AdaptiveBalanceSummary> items)
    {
        var views = new List<AdaptiveBalanceSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBalanceSummaryView(item));
        }
        return views;
    }

    public static double SumAdaptiveBalanceSummaryView(AdaptiveBalanceSummaryView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize + (double)parsedAttempts;
    }
}

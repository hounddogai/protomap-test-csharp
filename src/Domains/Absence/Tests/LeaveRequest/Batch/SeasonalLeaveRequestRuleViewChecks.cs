namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestRuleViewChecks
{
    public static void SeasonalLeaveRequestRuleViewShowsTheStatusInCapitals()
    {
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("code-620", "label-991");
        var view = SeasonalLeaveRequestRuleView.ToSeasonalLeaveRequestRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalLeaveRequestRuleView.ToSeasonalLeaveRequestRuleViews(new List<SeasonalLeaveRequestRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestRuleViewShowsTheStatusInCapitals();
    }
}

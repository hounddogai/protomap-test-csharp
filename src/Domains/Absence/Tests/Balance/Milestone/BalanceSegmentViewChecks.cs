namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceSegmentViewChecks
{
    public static void BalanceSegmentViewShowsTheStatusInCapitals()
    {
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
        var view = BalanceSegmentView.ToBalanceSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceSegmentView.ToBalanceSegmentViews(new List<BalanceSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceSegmentViewShowsTheStatusInCapitals();
    }
}

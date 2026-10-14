namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkMilestoneViewChecks
{
    public static void RegionalStatementOfWorkMilestoneViewShowsTheStatusInCapitals()
    {
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("code-395", "title-864");
        var view = RegionalStatementOfWorkMilestoneView.ToRegionalStatementOfWorkMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalStatementOfWorkMilestoneView.ToRegionalStatementOfWorkMilestoneViews(new List<RegionalStatementOfWorkMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkMilestoneViewShowsTheStatusInCapitals();
    }
}

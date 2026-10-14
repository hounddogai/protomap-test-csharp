namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AccrualMilestoneViewChecks
{
    public static void AccrualMilestoneViewShowsTheStatusInCapitals()
    {
        var item = AccrualMilestone.NewAccrualMilestone("code-260", "name-120");
        var view = AccrualMilestoneView.ToAccrualMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualMilestoneView.ToAccrualMilestoneViews(new List<AccrualMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualMilestoneViewShowsTheStatusInCapitals();
    }
}

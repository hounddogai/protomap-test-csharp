namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAssignmentViewChecks
{
    public static void AccrualAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
        var view = AccrualAssignmentView.ToAccrualAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualAssignmentView.ToAccrualAssignmentViews(new List<AccrualAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualAssignmentViewShowsTheStatusInCapitals();
    }
}

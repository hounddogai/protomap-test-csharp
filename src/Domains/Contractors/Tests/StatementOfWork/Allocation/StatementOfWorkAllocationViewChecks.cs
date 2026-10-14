namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAllocationViewChecks
{
    public static void StatementOfWorkAllocationViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkAllocation.NewStatementOfWorkAllocation("code-539", "name-955");
        var view = StatementOfWorkAllocationView.ToStatementOfWorkAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkAllocationView.ToStatementOfWorkAllocationViews(new List<StatementOfWorkAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkAllocationViewShowsTheStatusInCapitals();
    }
}

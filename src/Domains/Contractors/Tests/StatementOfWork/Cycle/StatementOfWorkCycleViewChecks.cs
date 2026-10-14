namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class StatementOfWorkCycleViewChecks
{
    public static void StatementOfWorkCycleViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkCycle.NewStatementOfWorkCycle("code-954", "label-200");
        var view = StatementOfWorkCycleView.ToStatementOfWorkCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkCycleView.ToStatementOfWorkCycleViews(new List<StatementOfWorkCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkCycleViewShowsTheStatusInCapitals();
    }
}

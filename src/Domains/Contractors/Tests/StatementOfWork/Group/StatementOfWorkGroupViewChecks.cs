namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class StatementOfWorkGroupViewChecks
{
    public static void StatementOfWorkGroupViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("code-226", "label-243");
        var view = StatementOfWorkGroupView.ToStatementOfWorkGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkGroupView.ToStatementOfWorkGroupViews(new List<StatementOfWorkGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkGroupViewShowsTheStatusInCapitals();
    }
}

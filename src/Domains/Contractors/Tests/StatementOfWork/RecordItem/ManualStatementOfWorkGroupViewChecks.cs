namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualStatementOfWorkGroupViewChecks
{
    public static void ManualStatementOfWorkGroupViewShowsTheStatusInCapitals()
    {
        var item = ManualStatementOfWorkGroup.NewManualStatementOfWorkGroup("code-453", "label-552");
        var view = ManualStatementOfWorkGroupView.ToManualStatementOfWorkGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualStatementOfWorkGroupView.ToManualStatementOfWorkGroupViews(new List<ManualStatementOfWorkGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualStatementOfWorkGroupViewShowsTheStatusInCapitals();
    }
}

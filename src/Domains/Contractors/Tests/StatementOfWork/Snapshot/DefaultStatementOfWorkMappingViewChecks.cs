namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMappingViewChecks
{
    public static void DefaultStatementOfWorkMappingViewShowsTheStatusInCapitals()
    {
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
        var view = DefaultStatementOfWorkMappingView.ToDefaultStatementOfWorkMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultStatementOfWorkMappingView.ToDefaultStatementOfWorkMappingViews(new List<DefaultStatementOfWorkMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMappingViewShowsTheStatusInCapitals();
    }
}

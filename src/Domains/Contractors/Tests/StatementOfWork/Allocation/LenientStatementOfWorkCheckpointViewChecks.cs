namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientStatementOfWorkCheckpointViewChecks
{
    public static void LenientStatementOfWorkCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
        var view = LenientStatementOfWorkCheckpointView.ToLenientStatementOfWorkCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientStatementOfWorkCheckpointView.ToLenientStatementOfWorkCheckpointViews(new List<LenientStatementOfWorkCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkCheckpointViewShowsTheStatusInCapitals();
    }
}

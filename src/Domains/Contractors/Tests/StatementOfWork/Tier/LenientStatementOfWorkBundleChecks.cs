namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientStatementOfWorkBundleChecks
{
    public static void LenientStatementOfWorkBundleStartsInPlanned()
    {
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("code-761", "name-201");
        Check.Equal(item.Status, LenientStatementOfWorkBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientStatementOfWorkBundleMovesToCancelled()
    {
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("code-761", "name-201");
        try
        {
            item.MoveTo(LenientStatementOfWorkBundleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientStatementOfWorkBundleStatus.Cancelled, "status");
    }

    public static void LenientStatementOfWorkBundleRejectsMovesFromCancelled()
    {
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("code-761", "name-201");
        item.Status = LenientStatementOfWorkBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientStatementOfWorkBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkBundleStartsInPlanned();
        LenientStatementOfWorkBundleMovesToCancelled();
        LenientStatementOfWorkBundleRejectsMovesFromCancelled();
    }
}

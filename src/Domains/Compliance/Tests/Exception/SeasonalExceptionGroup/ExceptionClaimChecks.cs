namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionClaimChecks
{
    public static void ExceptionClaimStartsInDraft()
    {
        var item = ExceptionClaim.NewExceptionClaim("code-868", "name-797");
        Check.Equal(item.Status, ExceptionClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionClaimMovesToClosed()
    {
        var item = ExceptionClaim.NewExceptionClaim("code-868", "name-797");
        try
        {
            item.MoveTo(ExceptionClaimStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionClaimStatus.Closed, "status");
    }

    public static void ExceptionClaimRejectsMovesFromClosed()
    {
        var item = ExceptionClaim.NewExceptionClaim("code-868", "name-797");
        item.Status = ExceptionClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionClaimStartsInDraft();
        ExceptionClaimMovesToClosed();
        ExceptionClaimRejectsMovesFromClosed();
    }
}

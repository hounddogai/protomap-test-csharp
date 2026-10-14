namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalPoolExceptionChecks
{
    public static void GlobalPoolExceptionStartsInCreated()
    {
        var item = GlobalPoolException.NewGlobalPoolException("code-478", "label-721");
        Check.Equal(item.Status, GlobalPoolExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPoolExceptionMovesToValidated()
    {
        var item = GlobalPoolException.NewGlobalPoolException("code-478", "label-721");
        try
        {
            item.MoveTo(GlobalPoolExceptionStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPoolExceptionStatus.Validated, "status");
    }

    public static void GlobalPoolExceptionRejectsMovesFromRetired()
    {
        var item = GlobalPoolException.NewGlobalPoolException("code-478", "label-721");
        item.Status = GlobalPoolExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalPoolExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalPoolExceptionStartsInCreated();
        GlobalPoolExceptionMovesToValidated();
        GlobalPoolExceptionRejectsMovesFromRetired();
    }
}

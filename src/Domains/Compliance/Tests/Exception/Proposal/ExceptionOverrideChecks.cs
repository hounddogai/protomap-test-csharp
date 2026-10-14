namespace Acme.Workforce.Compliance.Tests.Exception.Proposal;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ExceptionOverrideChecks
{
    public static void ExceptionOverrideStartsInNew()
    {
        var item = ExceptionOverride.NewExceptionOverride("code-161", "name-834");
        Check.Equal(item.Status, ExceptionOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionOverrideMovesToFailed()
    {
        var item = ExceptionOverride.NewExceptionOverride("code-161", "name-834");
        try
        {
            item.MoveTo(ExceptionOverrideStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionOverrideStatus.Failed, "status");
    }

    public static void ExceptionOverrideRejectsMovesFromFailed()
    {
        var item = ExceptionOverride.NewExceptionOverride("code-161", "name-834");
        item.Status = ExceptionOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionOverrideStartsInNew();
        ExceptionOverrideMovesToFailed();
        ExceptionOverrideRejectsMovesFromFailed();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ExceptionProfileChecks
{
    public static void ExceptionProfileStartsInNew()
    {
        var item = ExceptionProfile.NewExceptionProfile("code-888", "name-435");
        Check.Equal(item.Status, ExceptionProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionProfileMovesToQueued()
    {
        var item = ExceptionProfile.NewExceptionProfile("code-888", "name-435");
        try
        {
            item.MoveTo(ExceptionProfileStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionProfileStatus.Queued, "status");
    }

    public static void ExceptionProfileRejectsMovesFromFailed()
    {
        var item = ExceptionProfile.NewExceptionProfile("code-888", "name-435");
        item.Status = ExceptionProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionProfileStartsInNew();
        ExceptionProfileMovesToQueued();
        ExceptionProfileRejectsMovesFromFailed();
    }
}

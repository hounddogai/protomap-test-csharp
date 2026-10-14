namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ExceptionPreferenceChecks
{
    public static void ExceptionPreferenceStartsInIdle()
    {
        var item = ExceptionPreference.NewExceptionPreference("code-264", "title-371");
        Check.Equal(item.Status, ExceptionPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionPreferenceMovesToReady()
    {
        var item = ExceptionPreference.NewExceptionPreference("code-264", "title-371");
        try
        {
            item.MoveTo(ExceptionPreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionPreferenceStatus.Ready, "status");
    }

    public static void ExceptionPreferenceRejectsMovesFromStopped()
    {
        var item = ExceptionPreference.NewExceptionPreference("code-264", "title-371");
        item.Status = ExceptionPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(ExceptionPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ExceptionPreferenceStartsInIdle();
        ExceptionPreferenceMovesToReady();
        ExceptionPreferenceRejectsMovesFromStopped();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class LegacyExceptionProfileChecks
{
    public static void LegacyExceptionProfileStartsInRequested()
    {
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("code-294", "label-383");
        Check.Equal(item.Status, LegacyExceptionProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyExceptionProfileMovesToRevoked()
    {
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("code-294", "label-383");
        try
        {
            item.MoveTo(LegacyExceptionProfileStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyExceptionProfileStatus.Revoked, "status");
    }

    public static void LegacyExceptionProfileRejectsMovesFromExpired()
    {
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("code-294", "label-383");
        item.Status = LegacyExceptionProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyExceptionProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyExceptionProfileStartsInRequested();
        LegacyExceptionProfileMovesToRevoked();
        LegacyExceptionProfileRejectsMovesFromExpired();
    }
}

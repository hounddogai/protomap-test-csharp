namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientScenarioDraftChecks
{
    public static void LenientScenarioDraftStartsInRequested()
    {
        var item = LenientScenarioDraft.NewLenientScenarioDraft("code-926", "name-762");
        Check.Equal(item.Status, LenientScenarioDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientScenarioDraftMovesToGranted()
    {
        var item = LenientScenarioDraft.NewLenientScenarioDraft("code-926", "name-762");
        try
        {
            item.MoveTo(LenientScenarioDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientScenarioDraftStatus.Granted, "status");
    }

    public static void LenientScenarioDraftRejectsMovesFromExpired()
    {
        var item = LenientScenarioDraft.NewLenientScenarioDraft("code-926", "name-762");
        item.Status = LenientScenarioDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientScenarioDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientScenarioDraftStartsInRequested();
        LenientScenarioDraftMovesToGranted();
        LenientScenarioDraftRejectsMovesFromExpired();
    }
}

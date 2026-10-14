namespace Acme.Workforce.Orgdesign.Tests.Position.PositionEstimate;

using Acme.Workforce.Orgdesign.Position.PositionEstimate;

/// <summary>Checks of position estimate code, run by the project's runner.</summary>
public static class PositionWindowChecks
{
    public static void PositionWindowStartsInRequested()
    {
        var item = PositionWindow.NewPositionWindow("code-836", "name-297");
        Check.Equal(item.Status, PositionWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionWindowMovesToGranted()
    {
        var item = PositionWindow.NewPositionWindow("code-836", "name-297");
        try
        {
            item.MoveTo(PositionWindowStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionWindowStatus.Granted, "status");
    }

    public static void PositionWindowRejectsMovesFromExpired()
    {
        var item = PositionWindow.NewPositionWindow("code-836", "name-297");
        item.Status = PositionWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(PositionWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PositionWindowStartsInRequested();
        PositionWindowMovesToGranted();
        PositionWindowRejectsMovesFromExpired();
    }
}

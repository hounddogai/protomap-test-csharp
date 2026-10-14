namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionRollupChecks
{
    public static void PositionRollupStartsInRequested()
    {
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
        Check.Equal(item.Status, PositionRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionRollupMovesToGranted()
    {
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
        try
        {
            item.MoveTo(PositionRollupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionRollupStatus.Granted, "status");
    }

    public static void PositionRollupRejectsMovesFromExpired()
    {
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
        item.Status = PositionRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(PositionRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PositionRollupStartsInRequested();
        PositionRollupMovesToGranted();
        PositionRollupRejectsMovesFromExpired();
    }
}

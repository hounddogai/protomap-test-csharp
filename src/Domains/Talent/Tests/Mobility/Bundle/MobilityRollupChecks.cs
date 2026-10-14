namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityRollupChecks
{
    public static void MobilityRollupStartsInDraft()
    {
        var item = MobilityRollup.NewMobilityRollup("code-966", "title-546");
        Check.Equal(item.Status, MobilityRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityRollupMovesToSuspended()
    {
        var item = MobilityRollup.NewMobilityRollup("code-966", "title-546");
        try
        {
            item.MoveTo(MobilityRollupStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityRollupStatus.Suspended, "status");
    }

    public static void MobilityRollupRejectsMovesFromClosed()
    {
        var item = MobilityRollup.NewMobilityRollup("code-966", "title-546");
        item.Status = MobilityRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(MobilityRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MobilityRollupStartsInDraft();
        MobilityRollupMovesToSuspended();
        MobilityRollupRejectsMovesFromClosed();
    }
}

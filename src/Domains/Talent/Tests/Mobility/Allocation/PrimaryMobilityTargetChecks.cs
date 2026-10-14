namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PrimaryMobilityTargetChecks
{
    public static void PrimaryMobilityTargetStartsInDraft()
    {
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("code-734", "title-52");
        Check.Equal(item.Status, PrimaryMobilityTargetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMobilityTargetMovesToSuspended()
    {
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("code-734", "title-52");
        try
        {
            item.MoveTo(PrimaryMobilityTargetStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMobilityTargetStatus.Suspended, "status");
    }

    public static void PrimaryMobilityTargetRejectsMovesFromClosed()
    {
        var item = PrimaryMobilityTarget.NewPrimaryMobilityTarget("code-734", "title-52");
        item.Status = PrimaryMobilityTargetStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryMobilityTargetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryMobilityTargetStartsInDraft();
        PrimaryMobilityTargetMovesToSuspended();
        PrimaryMobilityTargetRejectsMovesFromClosed();
    }
}

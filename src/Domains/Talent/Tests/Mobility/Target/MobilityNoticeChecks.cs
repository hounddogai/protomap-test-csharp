namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityNoticeChecks
{
    public static void MobilityNoticeStartsInProposed()
    {
        var item = MobilityNotice.NewMobilityNotice("code-647", "name-250");
        Check.Equal(item.Status, MobilityNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityNoticeMovesToWithdrawn()
    {
        var item = MobilityNotice.NewMobilityNotice("code-647", "name-250");
        try
        {
            item.MoveTo(MobilityNoticeStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityNoticeStatus.Withdrawn, "status");
    }

    public static void MobilityNoticeRejectsMovesFromWithdrawn()
    {
        var item = MobilityNotice.NewMobilityNotice("code-647", "name-250");
        item.Status = MobilityNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MobilityNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MobilityNoticeStartsInProposed();
        MobilityNoticeMovesToWithdrawn();
        MobilityNoticeRejectsMovesFromWithdrawn();
    }
}

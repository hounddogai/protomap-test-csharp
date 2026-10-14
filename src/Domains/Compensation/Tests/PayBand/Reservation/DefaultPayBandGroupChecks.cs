namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandGroupChecks
{
    public static void DefaultPayBandGroupStartsInIdle()
    {
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("code-336", "title-491");
        Check.Equal(item.Status, DefaultPayBandGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayBandGroupMovesToReady()
    {
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("code-336", "title-491");
        try
        {
            item.MoveTo(DefaultPayBandGroupStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayBandGroupStatus.Ready, "status");
    }

    public static void DefaultPayBandGroupRejectsMovesFromStopped()
    {
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("code-336", "title-491");
        item.Status = DefaultPayBandGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultPayBandGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultPayBandGroupStartsInIdle();
        DefaultPayBandGroupMovesToReady();
        DefaultPayBandGroupRejectsMovesFromStopped();
    }
}

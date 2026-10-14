namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;

/// <summary>Checks of seat cycle code, run by the project's runner.</summary>
public static class LicenseSeatDigestChecks
{
    public static void LicenseSeatDigestStartsInPlanned()
    {
        var item = LicenseSeatDigest.NewLicenseSeatDigest("code-898", "name-414");
        Check.Equal(item.Status, LicenseSeatDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatDigestMovesToScheduled()
    {
        var item = LicenseSeatDigest.NewLicenseSeatDigest("code-898", "name-414");
        try
        {
            item.MoveTo(LicenseSeatDigestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatDigestStatus.Scheduled, "status");
    }

    public static void LicenseSeatDigestRejectsMovesFromCancelled()
    {
        var item = LicenseSeatDigest.NewLicenseSeatDigest("code-898", "name-414");
        item.Status = LicenseSeatDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LicenseSeatDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LicenseSeatDigestStartsInPlanned();
        LicenseSeatDigestMovesToScheduled();
        LicenseSeatDigestRejectsMovesFromCancelled();
    }
}

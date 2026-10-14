namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentDigestChecks
{
    public static void GarnishmentDigestStartsInIdle()
    {
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
        Check.Equal(item.Status, GarnishmentDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentDigestMovesToWarming()
    {
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
        try
        {
            item.MoveTo(GarnishmentDigestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentDigestStatus.Warming, "status");
    }

    public static void GarnishmentDigestRejectsMovesFromStopped()
    {
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
        item.Status = GarnishmentDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(GarnishmentDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GarnishmentDigestStartsInIdle();
        GarnishmentDigestMovesToWarming();
        GarnishmentDigestRejectsMovesFromStopped();
    }
}

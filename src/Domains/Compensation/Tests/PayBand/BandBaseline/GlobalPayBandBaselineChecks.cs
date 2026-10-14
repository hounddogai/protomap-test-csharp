namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class GlobalPayBandBaselineChecks
{
    public static void GlobalPayBandBaselineStartsInPlanned()
    {
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
        Check.Equal(item.Status, GlobalPayBandBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayBandBaselineMovesToCompleted()
    {
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
        try
        {
            item.MoveTo(GlobalPayBandBaselineStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayBandBaselineStatus.Completed, "status");
    }

    public static void GlobalPayBandBaselineRejectsMovesFromCancelled()
    {
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
        item.Status = GlobalPayBandBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalPayBandBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalPayBandBaselineStartsInPlanned();
        GlobalPayBandBaselineMovesToCompleted();
        GlobalPayBandBaselineRejectsMovesFromCancelled();
    }
}

namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalRetroPayTierChecks
{
    public static void RegionalRetroPayTierStartsInNew()
    {
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("code-184", "label-268");
        Check.Equal(item.Status, RegionalRetroPayTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalRetroPayTierMovesToRunning()
    {
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("code-184", "label-268");
        try
        {
            item.MoveTo(RegionalRetroPayTierStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalRetroPayTierStatus.Running, "status");
    }

    public static void RegionalRetroPayTierRejectsMovesFromFailed()
    {
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("code-184", "label-268");
        item.Status = RegionalRetroPayTierStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalRetroPayTierStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalRetroPayTierStartsInNew();
        RegionalRetroPayTierMovesToRunning();
        RegionalRetroPayTierRejectsMovesFromFailed();
    }
}

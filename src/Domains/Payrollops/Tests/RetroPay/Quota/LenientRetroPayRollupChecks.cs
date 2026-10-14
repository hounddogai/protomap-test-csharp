namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientRetroPayRollupChecks
{
    public static void LenientRetroPayRollupStartsInNew()
    {
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("code-201", "title-469");
        Check.Equal(item.Status, LenientRetroPayRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRetroPayRollupMovesToQueued()
    {
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("code-201", "title-469");
        try
        {
            item.MoveTo(LenientRetroPayRollupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRetroPayRollupStatus.Queued, "status");
    }

    public static void LenientRetroPayRollupRejectsMovesFromFailed()
    {
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("code-201", "title-469");
        item.Status = LenientRetroPayRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientRetroPayRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientRetroPayRollupStartsInNew();
        LenientRetroPayRollupMovesToQueued();
        LenientRetroPayRollupRejectsMovesFromFailed();
    }
}

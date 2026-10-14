namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolRequestChecks
{
    public static void FallbackBonusPoolRequestStartsInIdle()
    {
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("code-402", "name-545");
        Check.Equal(item.Status, FallbackBonusPoolRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBonusPoolRequestMovesToDraining()
    {
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("code-402", "name-545");
        try
        {
            item.MoveTo(FallbackBonusPoolRequestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBonusPoolRequestStatus.Draining, "status");
    }

    public static void FallbackBonusPoolRequestRejectsMovesFromStopped()
    {
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("code-402", "name-545");
        item.Status = FallbackBonusPoolRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackBonusPoolRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackBonusPoolRequestStartsInIdle();
        FallbackBonusPoolRequestMovesToDraining();
        FallbackBonusPoolRequestRejectsMovesFromStopped();
    }
}

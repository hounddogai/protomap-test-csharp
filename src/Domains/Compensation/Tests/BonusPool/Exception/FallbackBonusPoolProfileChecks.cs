namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackBonusPoolProfileChecks
{
    public static void FallbackBonusPoolProfileStartsInIdle()
    {
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("code-293", "title-813");
        Check.Equal(item.Status, FallbackBonusPoolProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBonusPoolProfileMovesToWarming()
    {
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("code-293", "title-813");
        try
        {
            item.MoveTo(FallbackBonusPoolProfileStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBonusPoolProfileStatus.Warming, "status");
    }

    public static void FallbackBonusPoolProfileRejectsMovesFromStopped()
    {
        var item = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile("code-293", "title-813");
        item.Status = FallbackBonusPoolProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackBonusPoolProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackBonusPoolProfileStartsInIdle();
        FallbackBonusPoolProfileMovesToWarming();
        FallbackBonusPoolProfileRejectsMovesFromStopped();
    }
}

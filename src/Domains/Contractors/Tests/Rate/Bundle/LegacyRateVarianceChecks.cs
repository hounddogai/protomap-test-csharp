namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyRateVarianceChecks
{
    public static void LegacyRateVarianceStartsInIdle()
    {
        var item = LegacyRateVariance.NewLegacyRateVariance("code-635", "name-336");
        Check.Equal(item.Status, LegacyRateVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRateVarianceMovesToWarming()
    {
        var item = LegacyRateVariance.NewLegacyRateVariance("code-635", "name-336");
        try
        {
            item.MoveTo(LegacyRateVarianceStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRateVarianceStatus.Warming, "status");
    }

    public static void LegacyRateVarianceRejectsMovesFromStopped()
    {
        var item = LegacyRateVariance.NewLegacyRateVariance("code-635", "name-336");
        item.Status = LegacyRateVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyRateVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyRateVarianceStartsInIdle();
        LegacyRateVarianceMovesToWarming();
        LegacyRateVarianceRejectsMovesFromStopped();
    }
}

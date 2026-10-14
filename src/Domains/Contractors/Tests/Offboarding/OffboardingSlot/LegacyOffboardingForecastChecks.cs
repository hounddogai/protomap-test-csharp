namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class LegacyOffboardingForecastChecks
{
    public static void LegacyOffboardingForecastStartsInProposed()
    {
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
        Check.Equal(item.Status, LegacyOffboardingForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOffboardingForecastMovesToApproved()
    {
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
        try
        {
            item.MoveTo(LegacyOffboardingForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOffboardingForecastStatus.Approved, "status");
    }

    public static void LegacyOffboardingForecastRejectsMovesFromWithdrawn()
    {
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
        item.Status = LegacyOffboardingForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyOffboardingForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyOffboardingForecastStartsInProposed();
        LegacyOffboardingForecastMovesToApproved();
        LegacyOffboardingForecastRejectsMovesFromWithdrawn();
    }
}

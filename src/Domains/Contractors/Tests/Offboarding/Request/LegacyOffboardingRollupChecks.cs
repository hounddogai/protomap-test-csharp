namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyOffboardingRollupChecks
{
    public static void LegacyOffboardingRollupStartsInDraft()
    {
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("code-226", "title-62");
        Check.Equal(item.Status, LegacyOffboardingRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOffboardingRollupMovesToClosed()
    {
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("code-226", "title-62");
        try
        {
            item.MoveTo(LegacyOffboardingRollupStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOffboardingRollupStatus.Closed, "status");
    }

    public static void LegacyOffboardingRollupRejectsMovesFromClosed()
    {
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("code-226", "title-62");
        item.Status = LegacyOffboardingRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyOffboardingRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyOffboardingRollupStartsInDraft();
        LegacyOffboardingRollupMovesToClosed();
        LegacyOffboardingRollupRejectsMovesFromClosed();
    }
}

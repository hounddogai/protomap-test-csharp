namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyOffboardingThresholdChecks
{
    public static void LegacyOffboardingThresholdStartsInProposed()
    {
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("code-539", "title-406");
        Check.Equal(item.Status, LegacyOffboardingThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOffboardingThresholdMovesToApproved()
    {
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("code-539", "title-406");
        try
        {
            item.MoveTo(LegacyOffboardingThresholdStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOffboardingThresholdStatus.Approved, "status");
    }

    public static void LegacyOffboardingThresholdRejectsMovesFromWithdrawn()
    {
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("code-539", "title-406");
        item.Status = LegacyOffboardingThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyOffboardingThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyOffboardingThresholdStartsInProposed();
        LegacyOffboardingThresholdMovesToApproved();
        LegacyOffboardingThresholdRejectsMovesFromWithdrawn();
    }
}

namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingAdjustmentChecks
{
    public static void OffboardingAdjustmentStartsInPlanned()
    {
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
        Check.Equal(item.Status, OffboardingAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingAdjustmentMovesToScheduled()
    {
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
        try
        {
            item.MoveTo(OffboardingAdjustmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingAdjustmentStatus.Scheduled, "status");
    }

    public static void OffboardingAdjustmentRejectsMovesFromCancelled()
    {
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
        item.Status = OffboardingAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingAdjustmentStartsInPlanned();
        OffboardingAdjustmentMovesToScheduled();
        OffboardingAdjustmentRejectsMovesFromCancelled();
    }
}

namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class OffboardingPlanChecks
{
    public static void OffboardingPlanStartsInRequested()
    {
        var item = OffboardingPlan.NewOffboardingPlan("code-69", "label-649");
        Check.Equal(item.Status, OffboardingPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingPlanMovesToExpired()
    {
        var item = OffboardingPlan.NewOffboardingPlan("code-69", "label-649");
        try
        {
            item.MoveTo(OffboardingPlanStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingPlanStatus.Expired, "status");
    }

    public static void OffboardingPlanRejectsMovesFromExpired()
    {
        var item = OffboardingPlan.NewOffboardingPlan("code-69", "label-649");
        item.Status = OffboardingPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(OffboardingPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OffboardingPlanStartsInRequested();
        OffboardingPlanMovesToExpired();
        OffboardingPlanRejectsMovesFromExpired();
    }
}

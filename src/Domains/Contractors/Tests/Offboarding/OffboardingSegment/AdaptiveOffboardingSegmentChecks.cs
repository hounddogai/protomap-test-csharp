namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingSegmentChecks
{
    public static void AdaptiveOffboardingSegmentStartsInPlanned()
    {
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
        Check.Equal(item.Status, AdaptiveOffboardingSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveOffboardingSegmentMovesToCancelled()
    {
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
        try
        {
            item.MoveTo(AdaptiveOffboardingSegmentStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveOffboardingSegmentStatus.Cancelled, "status");
    }

    public static void AdaptiveOffboardingSegmentRejectsMovesFromCancelled()
    {
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
        item.Status = AdaptiveOffboardingSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveOffboardingSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingSegmentStartsInPlanned();
        AdaptiveOffboardingSegmentMovesToCancelled();
        AdaptiveOffboardingSegmentRejectsMovesFromCancelled();
    }
}

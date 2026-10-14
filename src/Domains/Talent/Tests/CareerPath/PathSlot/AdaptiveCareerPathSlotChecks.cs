namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotChecks
{
    public static void AdaptiveCareerPathSlotStartsInNew()
    {
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
        Check.Equal(item.Status, AdaptiveCareerPathSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCareerPathSlotMovesToQueued()
    {
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
        try
        {
            item.MoveTo(AdaptiveCareerPathSlotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCareerPathSlotStatus.Queued, "status");
    }

    public static void AdaptiveCareerPathSlotRejectsMovesFromFailed()
    {
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
        item.Status = AdaptiveCareerPathSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveCareerPathSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathSlotStartsInNew();
        AdaptiveCareerPathSlotMovesToQueued();
        AdaptiveCareerPathSlotRejectsMovesFromFailed();
    }
}

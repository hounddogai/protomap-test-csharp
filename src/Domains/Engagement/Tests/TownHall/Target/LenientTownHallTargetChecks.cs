namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientTownHallTargetChecks
{
    public static void LenientTownHallTargetStartsInNew()
    {
        var item = LenientTownHallTarget.NewLenientTownHallTarget("code-533", "name-152");
        Check.Equal(item.Status, LenientTownHallTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTownHallTargetMovesToRunning()
    {
        var item = LenientTownHallTarget.NewLenientTownHallTarget("code-533", "name-152");
        try
        {
            item.MoveTo(LenientTownHallTargetStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTownHallTargetStatus.Running, "status");
    }

    public static void LenientTownHallTargetRejectsMovesFromFailed()
    {
        var item = LenientTownHallTarget.NewLenientTownHallTarget("code-533", "name-152");
        item.Status = LenientTownHallTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientTownHallTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientTownHallTargetStartsInNew();
        LenientTownHallTargetMovesToRunning();
        LenientTownHallTargetRejectsMovesFromFailed();
    }
}

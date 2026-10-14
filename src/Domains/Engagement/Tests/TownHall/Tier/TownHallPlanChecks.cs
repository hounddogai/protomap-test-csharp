namespace Acme.Workforce.Engagement.Tests.TownHall.Tier;

using Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TownHallPlanChecks
{
    public static void TownHallPlanStartsInOpen()
    {
        var item = TownHallPlan.NewTownHallPlan("code-498", "name-924");
        Check.Equal(item.Status, TownHallPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallPlanMovesToInReview()
    {
        var item = TownHallPlan.NewTownHallPlan("code-498", "name-924");
        try
        {
            item.MoveTo(TownHallPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallPlanStatus.InReview, "status");
    }

    public static void TownHallPlanRejectsMovesFromArchived()
    {
        var item = TownHallPlan.NewTownHallPlan("code-498", "name-924");
        item.Status = TownHallPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(TownHallPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        TownHallPlanStartsInOpen();
        TownHallPlanMovesToInReview();
        TownHallPlanRejectsMovesFromArchived();
    }
}

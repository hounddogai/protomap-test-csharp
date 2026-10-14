namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountCycleChecks
{
    public static void HeadcountCycleStartsInOpen()
    {
        var item = HeadcountCycle.NewHeadcountCycle("code-861", "label-943");
        Check.Equal(item.Status, HeadcountCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountCycleMovesToInReview()
    {
        var item = HeadcountCycle.NewHeadcountCycle("code-861", "label-943");
        try
        {
            item.MoveTo(HeadcountCycleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountCycleStatus.InReview, "status");
    }

    public static void HeadcountCycleRejectsMovesFromArchived()
    {
        var item = HeadcountCycle.NewHeadcountCycle("code-861", "label-943");
        item.Status = HeadcountCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(HeadcountCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        HeadcountCycleStartsInOpen();
        HeadcountCycleMovesToInReview();
        HeadcountCycleRejectsMovesFromArchived();
    }
}

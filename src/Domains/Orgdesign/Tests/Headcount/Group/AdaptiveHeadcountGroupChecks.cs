namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveHeadcountGroupChecks
{
    public static void AdaptiveHeadcountGroupStartsInOpen()
    {
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("code-405", "title-713");
        Check.Equal(item.Status, AdaptiveHeadcountGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveHeadcountGroupMovesToReopened()
    {
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("code-405", "title-713");
        try
        {
            item.MoveTo(AdaptiveHeadcountGroupStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveHeadcountGroupStatus.Reopened, "status");
    }

    public static void AdaptiveHeadcountGroupRejectsMovesFromArchived()
    {
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("code-405", "title-713");
        item.Status = AdaptiveHeadcountGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveHeadcountGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountGroupStartsInOpen();
        AdaptiveHeadcountGroupMovesToReopened();
        AdaptiveHeadcountGroupRejectsMovesFromArchived();
    }
}

namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountOverrideChecks
{
    public static void HeadcountOverrideStartsInOpen()
    {
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
        Check.Equal(item.Status, HeadcountOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountOverrideMovesToReopened()
    {
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
        try
        {
            item.MoveTo(HeadcountOverrideStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountOverrideStatus.Reopened, "status");
    }

    public static void HeadcountOverrideRejectsMovesFromArchived()
    {
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
        item.Status = HeadcountOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(HeadcountOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        HeadcountOverrideStartsInOpen();
        HeadcountOverrideMovesToReopened();
        HeadcountOverrideRejectsMovesFromArchived();
    }
}

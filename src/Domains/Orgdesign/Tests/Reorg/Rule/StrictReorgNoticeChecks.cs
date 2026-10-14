namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictReorgNoticeChecks
{
    public static void StrictReorgNoticeStartsInCreated()
    {
        var item = StrictReorgNotice.NewStrictReorgNotice("code-800", "label-717");
        Check.Equal(item.Status, StrictReorgNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReorgNoticeMovesToValidated()
    {
        var item = StrictReorgNotice.NewStrictReorgNotice("code-800", "label-717");
        try
        {
            item.MoveTo(StrictReorgNoticeStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReorgNoticeStatus.Validated, "status");
    }

    public static void StrictReorgNoticeRejectsMovesFromRetired()
    {
        var item = StrictReorgNotice.NewStrictReorgNotice("code-800", "label-717");
        item.Status = StrictReorgNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictReorgNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictReorgNoticeStartsInCreated();
        StrictReorgNoticeMovesToValidated();
        StrictReorgNoticeRejectsMovesFromRetired();
    }
}

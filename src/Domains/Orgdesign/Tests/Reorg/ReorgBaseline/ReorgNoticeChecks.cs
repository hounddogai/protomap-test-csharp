namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>Checks of reorg baseline code, run by the project's runner.</summary>
public static class ReorgNoticeChecks
{
    public static void ReorgNoticeStartsInCreated()
    {
        var item = ReorgNotice.NewReorgNotice("code-200", "title-493");
        Check.Equal(item.Status, ReorgNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgNoticeMovesToRetired()
    {
        var item = ReorgNotice.NewReorgNotice("code-200", "title-493");
        try
        {
            item.MoveTo(ReorgNoticeStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgNoticeStatus.Retired, "status");
    }

    public static void ReorgNoticeRejectsMovesFromRetired()
    {
        var item = ReorgNotice.NewReorgNotice("code-200", "title-493");
        item.Status = ReorgNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(ReorgNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReorgNoticeStartsInCreated();
        ReorgNoticeMovesToRetired();
        ReorgNoticeRejectsMovesFromRetired();
    }
}

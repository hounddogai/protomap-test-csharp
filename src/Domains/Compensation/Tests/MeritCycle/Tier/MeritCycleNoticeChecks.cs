namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeChecks
{
    public static void MeritCycleNoticeStartsInCreated()
    {
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
        Check.Equal(item.Status, MeritCycleNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleNoticeMovesToPublished()
    {
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
        try
        {
            item.MoveTo(MeritCycleNoticeStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleNoticeStatus.Published, "status");
    }

    public static void MeritCycleNoticeRejectsMovesFromRetired()
    {
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
        item.Status = MeritCycleNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(MeritCycleNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        MeritCycleNoticeStartsInCreated();
        MeritCycleNoticeMovesToPublished();
        MeritCycleNoticeRejectsMovesFromRetired();
    }
}

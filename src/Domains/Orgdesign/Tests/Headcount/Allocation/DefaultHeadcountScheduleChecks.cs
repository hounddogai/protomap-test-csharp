namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultHeadcountScheduleChecks
{
    public static void DefaultHeadcountScheduleStartsInRequested()
    {
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("code-81", "title-324");
        Check.Equal(item.Status, DefaultHeadcountScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountScheduleMovesToRevoked()
    {
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("code-81", "title-324");
        try
        {
            item.MoveTo(DefaultHeadcountScheduleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountScheduleStatus.Revoked, "status");
    }

    public static void DefaultHeadcountScheduleRejectsMovesFromExpired()
    {
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("code-81", "title-324");
        item.Status = DefaultHeadcountScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultHeadcountScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountScheduleStartsInRequested();
        DefaultHeadcountScheduleMovesToRevoked();
        DefaultHeadcountScheduleRejectsMovesFromExpired();
    }
}

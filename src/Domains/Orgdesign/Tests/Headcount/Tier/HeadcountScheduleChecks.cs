namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountScheduleChecks
{
    public static void HeadcountScheduleStartsInProposed()
    {
        var item = HeadcountSchedule.NewHeadcountSchedule("code-745", "label-489");
        Check.Equal(item.Status, HeadcountScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountScheduleMovesToApproved()
    {
        var item = HeadcountSchedule.NewHeadcountSchedule("code-745", "label-489");
        try
        {
            item.MoveTo(HeadcountScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountScheduleStatus.Approved, "status");
    }

    public static void HeadcountScheduleRejectsMovesFromWithdrawn()
    {
        var item = HeadcountSchedule.NewHeadcountSchedule("code-745", "label-489");
        item.Status = HeadcountScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HeadcountScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HeadcountScheduleStartsInProposed();
        HeadcountScheduleMovesToApproved();
        HeadcountScheduleRejectsMovesFromWithdrawn();
    }
}

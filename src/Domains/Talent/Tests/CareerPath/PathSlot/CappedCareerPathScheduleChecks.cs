namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class CappedCareerPathScheduleChecks
{
    public static void CappedCareerPathScheduleStartsInPlanned()
    {
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("code-695", "label-270");
        Check.Equal(item.Status, CappedCareerPathScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCareerPathScheduleMovesToCompleted()
    {
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("code-695", "label-270");
        try
        {
            item.MoveTo(CappedCareerPathScheduleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCareerPathScheduleStatus.Completed, "status");
    }

    public static void CappedCareerPathScheduleRejectsMovesFromCancelled()
    {
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("code-695", "label-270");
        item.Status = CappedCareerPathScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedCareerPathScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedCareerPathScheduleStartsInPlanned();
        CappedCareerPathScheduleMovesToCompleted();
        CappedCareerPathScheduleRejectsMovesFromCancelled();
    }
}

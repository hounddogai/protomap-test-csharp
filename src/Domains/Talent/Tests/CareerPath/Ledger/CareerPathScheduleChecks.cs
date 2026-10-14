namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathScheduleChecks
{
    public static void CareerPathScheduleStartsInIdle()
    {
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
        Check.Equal(item.Status, CareerPathScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathScheduleMovesToReady()
    {
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
        try
        {
            item.MoveTo(CareerPathScheduleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathScheduleStatus.Ready, "status");
    }

    public static void CareerPathScheduleRejectsMovesFromStopped()
    {
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
        item.Status = CareerPathScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(CareerPathScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CareerPathScheduleStartsInIdle();
        CareerPathScheduleMovesToReady();
        CareerPathScheduleRejectsMovesFromStopped();
    }
}

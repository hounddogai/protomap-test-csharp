namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleChecks
{
    public static void GarnishmentScheduleStartsInOpen()
    {
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
        Check.Equal(item.Status, GarnishmentScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentScheduleMovesToReopened()
    {
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
        try
        {
            item.MoveTo(GarnishmentScheduleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentScheduleStatus.Reopened, "status");
    }

    public static void GarnishmentScheduleRejectsMovesFromArchived()
    {
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
        item.Status = GarnishmentScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(GarnishmentScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        GarnishmentScheduleStartsInOpen();
        GarnishmentScheduleMovesToReopened();
        GarnishmentScheduleRejectsMovesFromArchived();
    }
}

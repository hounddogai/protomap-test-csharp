namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class GarnishmentGroupChecks
{
    public static void GarnishmentGroupStartsInPlanned()
    {
        var item = GarnishmentGroup.NewGarnishmentGroup("code-915", "name-903");
        Check.Equal(item.Status, GarnishmentGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentGroupMovesToScheduled()
    {
        var item = GarnishmentGroup.NewGarnishmentGroup("code-915", "name-903");
        try
        {
            item.MoveTo(GarnishmentGroupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentGroupStatus.Scheduled, "status");
    }

    public static void GarnishmentGroupRejectsMovesFromCancelled()
    {
        var item = GarnishmentGroup.NewGarnishmentGroup("code-915", "name-903");
        item.Status = GarnishmentGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GarnishmentGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GarnishmentGroupStartsInPlanned();
        GarnishmentGroupMovesToScheduled();
        GarnishmentGroupRejectsMovesFromCancelled();
    }
}

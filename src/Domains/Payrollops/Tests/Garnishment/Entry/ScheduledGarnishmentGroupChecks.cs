namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ScheduledGarnishmentGroupChecks
{
    public static void ScheduledGarnishmentGroupStartsInCreated()
    {
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("code-236", "name-743");
        Check.Equal(item.Status, ScheduledGarnishmentGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledGarnishmentGroupMovesToValidated()
    {
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("code-236", "name-743");
        try
        {
            item.MoveTo(ScheduledGarnishmentGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledGarnishmentGroupStatus.Validated, "status");
    }

    public static void ScheduledGarnishmentGroupRejectsMovesFromRetired()
    {
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("code-236", "name-743");
        item.Status = ScheduledGarnishmentGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledGarnishmentGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentGroupStartsInCreated();
        ScheduledGarnishmentGroupMovesToValidated();
        ScheduledGarnishmentGroupRejectsMovesFromRetired();
    }
}

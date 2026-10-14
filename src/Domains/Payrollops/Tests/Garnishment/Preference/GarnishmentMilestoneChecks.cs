namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class GarnishmentMilestoneChecks
{
    public static void GarnishmentMilestoneStartsInOpen()
    {
        var item = GarnishmentMilestone.NewGarnishmentMilestone("code-142", "label-896");
        Check.Equal(item.Status, GarnishmentMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentMilestoneMovesToResolved()
    {
        var item = GarnishmentMilestone.NewGarnishmentMilestone("code-142", "label-896");
        try
        {
            item.MoveTo(GarnishmentMilestoneStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentMilestoneStatus.Resolved, "status");
    }

    public static void GarnishmentMilestoneRejectsMovesFromArchived()
    {
        var item = GarnishmentMilestone.NewGarnishmentMilestone("code-142", "label-896");
        item.Status = GarnishmentMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(GarnishmentMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        GarnishmentMilestoneStartsInOpen();
        GarnishmentMilestoneMovesToResolved();
        GarnishmentMilestoneRejectsMovesFromArchived();
    }
}

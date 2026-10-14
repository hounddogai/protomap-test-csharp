namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatMilestoneChecks
{
    public static void LicenseSeatMilestoneStartsInCreated()
    {
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("code-751", "label-619");
        Check.Equal(item.Status, LicenseSeatMilestoneStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatMilestoneMovesToPublished()
    {
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("code-751", "label-619");
        try
        {
            item.MoveTo(LicenseSeatMilestoneStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatMilestoneStatus.Published, "status");
    }

    public static void LicenseSeatMilestoneRejectsMovesFromRetired()
    {
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("code-751", "label-619");
        item.Status = LicenseSeatMilestoneStatus.Retired;
        Check.Fails(() => item.MoveTo(LicenseSeatMilestoneStatus.Created), "move");
    }

    public static void RunAll()
    {
        LicenseSeatMilestoneStartsInCreated();
        LicenseSeatMilestoneMovesToPublished();
        LicenseSeatMilestoneRejectsMovesFromRetired();
    }
}

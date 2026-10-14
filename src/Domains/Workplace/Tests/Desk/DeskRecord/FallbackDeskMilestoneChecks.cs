namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class FallbackDeskMilestoneChecks
{
    public static void FallbackDeskMilestoneStartsInCreated()
    {
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("code-454", "title-541");
        Check.Equal(item.Status, FallbackDeskMilestoneStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackDeskMilestoneMovesToValidated()
    {
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("code-454", "title-541");
        try
        {
            item.MoveTo(FallbackDeskMilestoneStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackDeskMilestoneStatus.Validated, "status");
    }

    public static void FallbackDeskMilestoneRejectsMovesFromRetired()
    {
        var item = FallbackDeskMilestone.NewFallbackDeskMilestone("code-454", "title-541");
        item.Status = FallbackDeskMilestoneStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackDeskMilestoneStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackDeskMilestoneStartsInCreated();
        FallbackDeskMilestoneMovesToValidated();
        FallbackDeskMilestoneRejectsMovesFromRetired();
    }
}

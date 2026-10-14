namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayMilestoneChecks
{
    public static void RetroPayMilestoneStartsInOpen()
    {
        var item = RetroPayMilestone.NewRetroPayMilestone("code-388", "label-926");
        Check.Equal(item.Status, RetroPayMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayMilestoneMovesToInReview()
    {
        var item = RetroPayMilestone.NewRetroPayMilestone("code-388", "label-926");
        try
        {
            item.MoveTo(RetroPayMilestoneStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayMilestoneStatus.InReview, "status");
    }

    public static void RetroPayMilestoneRejectsMovesFromArchived()
    {
        var item = RetroPayMilestone.NewRetroPayMilestone("code-388", "label-926");
        item.Status = RetroPayMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(RetroPayMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        RetroPayMilestoneStartsInOpen();
        RetroPayMilestoneMovesToInReview();
        RetroPayMilestoneRejectsMovesFromArchived();
    }
}

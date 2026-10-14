namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandMilestoneChecks
{
    public static void PayBandMilestoneStartsInIdle()
    {
        var item = PayBandMilestone.NewPayBandMilestone("code-715", "title-723");
        Check.Equal(item.Status, PayBandMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandMilestoneMovesToWarming()
    {
        var item = PayBandMilestone.NewPayBandMilestone("code-715", "title-723");
        try
        {
            item.MoveTo(PayBandMilestoneStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandMilestoneStatus.Warming, "status");
    }

    public static void PayBandMilestoneRejectsMovesFromStopped()
    {
        var item = PayBandMilestone.NewPayBandMilestone("code-715", "title-723");
        item.Status = PayBandMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayBandMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayBandMilestoneStartsInIdle();
        PayBandMilestoneMovesToWarming();
        PayBandMilestoneRejectsMovesFromStopped();
    }
}

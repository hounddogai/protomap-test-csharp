namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScheduledDeductionCycleChecks
{
    public static void ScheduledDeductionCycleStartsInProposed()
    {
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("code-888", "title-178");
        Check.Equal(item.Status, ScheduledDeductionCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledDeductionCycleMovesToRejected()
    {
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("code-888", "title-178");
        try
        {
            item.MoveTo(ScheduledDeductionCycleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledDeductionCycleStatus.Rejected, "status");
    }

    public static void ScheduledDeductionCycleRejectsMovesFromWithdrawn()
    {
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("code-888", "title-178");
        item.Status = ScheduledDeductionCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledDeductionCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledDeductionCycleStartsInProposed();
        ScheduledDeductionCycleMovesToRejected();
        ScheduledDeductionCycleRejectsMovesFromWithdrawn();
    }
}

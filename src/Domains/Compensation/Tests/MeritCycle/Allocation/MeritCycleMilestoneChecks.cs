namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleMilestoneChecks
{
    public static void MeritCycleMilestoneStartsInRequested()
    {
        var item = MeritCycleMilestone.NewMeritCycleMilestone("code-178", "label-369");
        Check.Equal(item.Status, MeritCycleMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleMilestoneMovesToGranted()
    {
        var item = MeritCycleMilestone.NewMeritCycleMilestone("code-178", "label-369");
        try
        {
            item.MoveTo(MeritCycleMilestoneStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleMilestoneStatus.Granted, "status");
    }

    public static void MeritCycleMilestoneRejectsMovesFromExpired()
    {
        var item = MeritCycleMilestone.NewMeritCycleMilestone("code-178", "label-369");
        item.Status = MeritCycleMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(MeritCycleMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MeritCycleMilestoneStartsInRequested();
        MeritCycleMilestoneMovesToGranted();
        MeritCycleMilestoneRejectsMovesFromExpired();
    }
}

namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationAssignmentChecks
{
    public static void NominationAssignmentStartsInIdle()
    {
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
        Check.Equal(item.Status, NominationAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationAssignmentMovesToWarming()
    {
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
        try
        {
            item.MoveTo(NominationAssignmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationAssignmentStatus.Warming, "status");
    }

    public static void NominationAssignmentRejectsMovesFromStopped()
    {
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
        item.Status = NominationAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(NominationAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        NominationAssignmentStartsInIdle();
        NominationAssignmentMovesToWarming();
        NominationAssignmentRejectsMovesFromStopped();
    }
}

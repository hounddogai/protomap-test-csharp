namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedNominationAssignmentChecks
{
    public static void CappedNominationAssignmentStartsInNew()
    {
        var item = CappedNominationAssignment.NewCappedNominationAssignment("code-261", "name-641");
        Check.Equal(item.Status, CappedNominationAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedNominationAssignmentMovesToFailed()
    {
        var item = CappedNominationAssignment.NewCappedNominationAssignment("code-261", "name-641");
        try
        {
            item.MoveTo(CappedNominationAssignmentStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedNominationAssignmentStatus.Failed, "status");
    }

    public static void CappedNominationAssignmentRejectsMovesFromFailed()
    {
        var item = CappedNominationAssignment.NewCappedNominationAssignment("code-261", "name-641");
        item.Status = CappedNominationAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedNominationAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedNominationAssignmentStartsInNew();
        CappedNominationAssignmentMovesToFailed();
        CappedNominationAssignmentRejectsMovesFromFailed();
    }
}

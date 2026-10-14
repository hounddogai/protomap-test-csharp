namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementChecks
{
    public static void AssignmentAgreementStartsInPlanned()
    {
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
        Check.Equal(item.Status, AssignmentAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentAgreementMovesToScheduled()
    {
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
        try
        {
            item.MoveTo(AssignmentAgreementStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentAgreementStatus.Scheduled, "status");
    }

    public static void AssignmentAgreementRejectsMovesFromCancelled()
    {
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
        item.Status = AssignmentAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssignmentAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssignmentAgreementStartsInPlanned();
        AssignmentAgreementMovesToScheduled();
        AssignmentAgreementRejectsMovesFromCancelled();
    }
}

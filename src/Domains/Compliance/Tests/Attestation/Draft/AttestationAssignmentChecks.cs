namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationAssignmentChecks
{
    public static void AttestationAssignmentStartsInProposed()
    {
        var item = AttestationAssignment.NewAttestationAssignment("code-936", "label-237");
        Check.Equal(item.Status, AttestationAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationAssignmentMovesToApproved()
    {
        var item = AttestationAssignment.NewAttestationAssignment("code-936", "label-237");
        try
        {
            item.MoveTo(AttestationAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationAssignmentStatus.Approved, "status");
    }

    public static void AttestationAssignmentRejectsMovesFromWithdrawn()
    {
        var item = AttestationAssignment.NewAttestationAssignment("code-936", "label-237");
        item.Status = AttestationAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AttestationAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AttestationAssignmentStartsInProposed();
        AttestationAssignmentMovesToApproved();
        AttestationAssignmentRejectsMovesFromWithdrawn();
    }
}

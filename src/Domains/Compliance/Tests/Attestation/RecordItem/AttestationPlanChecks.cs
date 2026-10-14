namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AttestationPlanChecks
{
    public static void AttestationPlanStartsInDraft()
    {
        var item = AttestationPlan.NewAttestationPlan("code-759", "title-681");
        Check.Equal(item.Status, AttestationPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationPlanMovesToActive()
    {
        var item = AttestationPlan.NewAttestationPlan("code-759", "title-681");
        try
        {
            item.MoveTo(AttestationPlanStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationPlanStatus.Active, "status");
    }

    public static void AttestationPlanRejectsMovesFromClosed()
    {
        var item = AttestationPlan.NewAttestationPlan("code-759", "title-681");
        item.Status = AttestationPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(AttestationPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AttestationPlanStartsInDraft();
        AttestationPlanMovesToActive();
        AttestationPlanRejectsMovesFromClosed();
    }
}

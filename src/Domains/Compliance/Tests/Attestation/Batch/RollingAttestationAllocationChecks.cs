namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RollingAttestationAllocationChecks
{
    public static void RollingAttestationAllocationStartsInOpen()
    {
        var item = RollingAttestationAllocation.NewRollingAttestationAllocation("code-437", "label-135");
        Check.Equal(item.Status, RollingAttestationAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAttestationAllocationMovesToArchived()
    {
        var item = RollingAttestationAllocation.NewRollingAttestationAllocation("code-437", "label-135");
        try
        {
            item.MoveTo(RollingAttestationAllocationStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAttestationAllocationStatus.Archived, "status");
    }

    public static void RollingAttestationAllocationRejectsMovesFromArchived()
    {
        var item = RollingAttestationAllocation.NewRollingAttestationAllocation("code-437", "label-135");
        item.Status = RollingAttestationAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingAttestationAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingAttestationAllocationStartsInOpen();
        RollingAttestationAllocationMovesToArchived();
        RollingAttestationAllocationRejectsMovesFromArchived();
    }
}

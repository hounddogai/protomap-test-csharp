namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationBudgetChecks
{
    public static void StrictAttestationBudgetStartsInIdle()
    {
        var item = StrictAttestationBudget.NewStrictAttestationBudget("code-491", "label-945");
        Check.Equal(item.Status, StrictAttestationBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAttestationBudgetMovesToWarming()
    {
        var item = StrictAttestationBudget.NewStrictAttestationBudget("code-491", "label-945");
        try
        {
            item.MoveTo(StrictAttestationBudgetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAttestationBudgetStatus.Warming, "status");
    }

    public static void StrictAttestationBudgetRejectsMovesFromStopped()
    {
        var item = StrictAttestationBudget.NewStrictAttestationBudget("code-491", "label-945");
        item.Status = StrictAttestationBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictAttestationBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictAttestationBudgetStartsInIdle();
        StrictAttestationBudgetMovesToWarming();
        StrictAttestationBudgetRejectsMovesFromStopped();
    }
}

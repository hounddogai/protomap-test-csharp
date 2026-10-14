namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LenientScenarioAgreementChecks
{
    public static void LenientScenarioAgreementStartsInProposed()
    {
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("code-47", "label-400");
        Check.Equal(item.Status, LenientScenarioAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientScenarioAgreementMovesToApproved()
    {
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("code-47", "label-400");
        try
        {
            item.MoveTo(LenientScenarioAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientScenarioAgreementStatus.Approved, "status");
    }

    public static void LenientScenarioAgreementRejectsMovesFromWithdrawn()
    {
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("code-47", "label-400");
        item.Status = LenientScenarioAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientScenarioAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientScenarioAgreementStartsInProposed();
        LenientScenarioAgreementMovesToApproved();
        LenientScenarioAgreementRejectsMovesFromWithdrawn();
    }
}

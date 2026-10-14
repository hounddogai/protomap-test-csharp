namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementChecks
{
    public static void PendingScenarioAgreementStartsInRequested()
    {
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
        Check.Equal(item.Status, PendingScenarioAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingScenarioAgreementMovesToGranted()
    {
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
        try
        {
            item.MoveTo(PendingScenarioAgreementStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingScenarioAgreementStatus.Granted, "status");
    }

    public static void PendingScenarioAgreementRejectsMovesFromExpired()
    {
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
        item.Status = PendingScenarioAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingScenarioAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingScenarioAgreementStartsInRequested();
        PendingScenarioAgreementMovesToGranted();
        PendingScenarioAgreementRejectsMovesFromExpired();
    }
}

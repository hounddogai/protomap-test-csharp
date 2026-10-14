namespace Acme.Workforce.Orgdesign.Tests.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Checks of weighted scenario template code, run by the project's runner.</summary>
public static class PrimaryScenarioAgreementChecks
{
    public static void PrimaryScenarioAgreementStartsInRequested()
    {
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("code-927", "label-906");
        Check.Equal(item.Status, PrimaryScenarioAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryScenarioAgreementMovesToGranted()
    {
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("code-927", "label-906");
        try
        {
            item.MoveTo(PrimaryScenarioAgreementStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryScenarioAgreementStatus.Granted, "status");
    }

    public static void PrimaryScenarioAgreementRejectsMovesFromExpired()
    {
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("code-927", "label-906");
        item.Status = PrimaryScenarioAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryScenarioAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryScenarioAgreementStartsInRequested();
        PrimaryScenarioAgreementMovesToGranted();
        PrimaryScenarioAgreementRejectsMovesFromExpired();
    }
}

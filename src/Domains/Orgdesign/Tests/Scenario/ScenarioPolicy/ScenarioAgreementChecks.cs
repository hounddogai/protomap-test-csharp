namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioAgreementChecks
{
    public static void ScenarioAgreementStartsInNew()
    {
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
        Check.Equal(item.Status, ScenarioAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioAgreementMovesToQueued()
    {
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
        try
        {
            item.MoveTo(ScenarioAgreementStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioAgreementStatus.Queued, "status");
    }

    public static void ScenarioAgreementRejectsMovesFromFailed()
    {
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
        item.Status = ScenarioAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(ScenarioAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        ScenarioAgreementStartsInNew();
        ScenarioAgreementMovesToQueued();
        ScenarioAgreementRejectsMovesFromFailed();
    }
}

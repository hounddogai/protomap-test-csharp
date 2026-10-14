namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioProposalChecks
{
    public static void ScenarioProposalStartsInIdle()
    {
        var item = ScenarioProposal.NewScenarioProposal("code-852", "title-363");
        Check.Equal(item.Status, ScenarioProposalStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioProposalMovesToStopped()
    {
        var item = ScenarioProposal.NewScenarioProposal("code-852", "title-363");
        try
        {
            item.MoveTo(ScenarioProposalStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioProposalStatus.Stopped, "status");
    }

    public static void ScenarioProposalRejectsMovesFromStopped()
    {
        var item = ScenarioProposal.NewScenarioProposal("code-852", "title-363");
        item.Status = ScenarioProposalStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScenarioProposalStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScenarioProposalStartsInIdle();
        ScenarioProposalMovesToStopped();
        ScenarioProposalRejectsMovesFromStopped();
    }
}

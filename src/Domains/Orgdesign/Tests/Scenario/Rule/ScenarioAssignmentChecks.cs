namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioAssignmentChecks
{
    public static void ScenarioAssignmentStartsInCreated()
    {
        var item = ScenarioAssignment.NewScenarioAssignment("code-972", "label-199");
        Check.Equal(item.Status, ScenarioAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioAssignmentMovesToRetired()
    {
        var item = ScenarioAssignment.NewScenarioAssignment("code-972", "label-199");
        try
        {
            item.MoveTo(ScenarioAssignmentStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioAssignmentStatus.Retired, "status");
    }

    public static void ScenarioAssignmentRejectsMovesFromRetired()
    {
        var item = ScenarioAssignment.NewScenarioAssignment("code-972", "label-199");
        item.Status = ScenarioAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioAssignmentStartsInCreated();
        ScenarioAssignmentMovesToRetired();
        ScenarioAssignmentRejectsMovesFromRetired();
    }
}

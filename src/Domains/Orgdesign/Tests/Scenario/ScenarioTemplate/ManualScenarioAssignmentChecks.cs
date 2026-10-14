namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAssignmentChecks
{
    public static void ManualScenarioAssignmentStartsInCreated()
    {
        var item = ManualScenarioAssignment.NewManualScenarioAssignment("code-442", "title-539");
        Check.Equal(item.Status, ManualScenarioAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualScenarioAssignmentMovesToValidated()
    {
        var item = ManualScenarioAssignment.NewManualScenarioAssignment("code-442", "title-539");
        try
        {
            item.MoveTo(ManualScenarioAssignmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualScenarioAssignmentStatus.Validated, "status");
    }

    public static void ManualScenarioAssignmentRejectsMovesFromRetired()
    {
        var item = ManualScenarioAssignment.NewManualScenarioAssignment("code-442", "title-539");
        item.Status = ManualScenarioAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualScenarioAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualScenarioAssignmentStartsInCreated();
        ManualScenarioAssignmentMovesToValidated();
        ManualScenarioAssignmentRejectsMovesFromRetired();
    }
}

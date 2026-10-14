namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PrimaryScenarioMappingChecks
{
    public static void PrimaryScenarioMappingStartsInPlanned()
    {
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("code-657", "label-827");
        Check.Equal(item.Status, PrimaryScenarioMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryScenarioMappingMovesToScheduled()
    {
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("code-657", "label-827");
        try
        {
            item.MoveTo(PrimaryScenarioMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryScenarioMappingStatus.Scheduled, "status");
    }

    public static void PrimaryScenarioMappingRejectsMovesFromCancelled()
    {
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("code-657", "label-827");
        item.Status = PrimaryScenarioMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryScenarioMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryScenarioMappingStartsInPlanned();
        PrimaryScenarioMappingMovesToScheduled();
        PrimaryScenarioMappingRejectsMovesFromCancelled();
    }
}

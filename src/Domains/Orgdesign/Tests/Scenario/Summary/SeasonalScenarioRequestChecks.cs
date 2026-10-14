namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SeasonalScenarioRequestChecks
{
    public static void SeasonalScenarioRequestStartsInCreated()
    {
        var item = SeasonalScenarioRequest.NewSeasonalScenarioRequest("code-406", "label-521");
        Check.Equal(item.Status, SeasonalScenarioRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalScenarioRequestMovesToValidated()
    {
        var item = SeasonalScenarioRequest.NewSeasonalScenarioRequest("code-406", "label-521");
        try
        {
            item.MoveTo(SeasonalScenarioRequestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalScenarioRequestStatus.Validated, "status");
    }

    public static void SeasonalScenarioRequestRejectsMovesFromRetired()
    {
        var item = SeasonalScenarioRequest.NewSeasonalScenarioRequest("code-406", "label-521");
        item.Status = SeasonalScenarioRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalScenarioRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalScenarioRequestStartsInCreated();
        SeasonalScenarioRequestMovesToValidated();
        SeasonalScenarioRequestRejectsMovesFromRetired();
    }
}

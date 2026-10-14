namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioQuotaChecks
{
    public static void ScenarioQuotaStartsInPlanned()
    {
        var item = ScenarioQuota.NewScenarioQuota("code-158", "title-116");
        Check.Equal(item.Status, ScenarioQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioQuotaMovesToScheduled()
    {
        var item = ScenarioQuota.NewScenarioQuota("code-158", "title-116");
        try
        {
            item.MoveTo(ScenarioQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioQuotaStatus.Scheduled, "status");
    }

    public static void ScenarioQuotaRejectsMovesFromCancelled()
    {
        var item = ScenarioQuota.NewScenarioQuota("code-158", "title-116");
        item.Status = ScenarioQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScenarioQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScenarioQuotaStartsInPlanned();
        ScenarioQuotaMovesToScheduled();
        ScenarioQuotaRejectsMovesFromCancelled();
    }
}

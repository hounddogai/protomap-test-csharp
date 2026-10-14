namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class StrictScenarioQuotaChecks
{
    public static void StrictScenarioQuotaStartsInPlanned()
    {
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
        Check.Equal(item.Status, StrictScenarioQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictScenarioQuotaMovesToScheduled()
    {
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
        try
        {
            item.MoveTo(StrictScenarioQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictScenarioQuotaStatus.Scheduled, "status");
    }

    public static void StrictScenarioQuotaRejectsMovesFromCancelled()
    {
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
        item.Status = StrictScenarioQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictScenarioQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictScenarioQuotaStartsInPlanned();
        StrictScenarioQuotaMovesToScheduled();
        StrictScenarioQuotaRejectsMovesFromCancelled();
    }
}

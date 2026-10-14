namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class StrictScenarioQuotaViewChecks
{
    public static void StrictScenarioQuotaViewShowsTheStatusInCapitals()
    {
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
        var view = StrictScenarioQuotaView.ToStrictScenarioQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictScenarioQuotaView.ToStrictScenarioQuotaViews(new List<StrictScenarioQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictScenarioQuotaViewShowsTheStatusInCapitals();
    }
}

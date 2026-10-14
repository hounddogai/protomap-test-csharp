namespace Acme.Workforce.Orgdesign.Tests.Scenario.Template;

using Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScenarioLedgerViewChecks
{
    public static void ScenarioLedgerViewShowsTheStatusInCapitals()
    {
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
        var view = ScenarioLedgerView.ToScenarioLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioLedgerView.ToScenarioLedgerViews(new List<ScenarioLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioLedgerViewShowsTheStatusInCapitals();
    }
}

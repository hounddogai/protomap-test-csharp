namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPayRunTargetViewChecks
{
    public static void LegacyPayRunTargetViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("code-587", "name-357");
        var view = LegacyPayRunTargetView.ToLegacyPayRunTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayRunTargetView.ToLegacyPayRunTargetViews(new List<LegacyPayRunTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayRunTargetViewShowsTheStatusInCapitals();
    }
}

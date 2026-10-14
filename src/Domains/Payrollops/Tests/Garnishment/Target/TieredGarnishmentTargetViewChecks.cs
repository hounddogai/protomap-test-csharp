namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredGarnishmentTargetViewChecks
{
    public static void TieredGarnishmentTargetViewShowsTheStatusInCapitals()
    {
        var item = TieredGarnishmentTarget.NewTieredGarnishmentTarget("code-951", "label-982");
        var view = TieredGarnishmentTargetView.ToTieredGarnishmentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredGarnishmentTargetView.ToTieredGarnishmentTargetViews(new List<TieredGarnishmentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredGarnishmentTargetViewShowsTheStatusInCapitals();
    }
}

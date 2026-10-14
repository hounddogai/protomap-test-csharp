namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetViewChecks
{
    public static void GarnishmentTargetViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
        var view = GarnishmentTargetView.ToGarnishmentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentTargetView.ToGarnishmentTargetViews(new List<GarnishmentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentTargetViewShowsTheStatusInCapitals();
    }
}

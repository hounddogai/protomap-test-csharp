namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredLicenseSeatRuleViewChecks
{
    public static void TieredLicenseSeatRuleViewShowsTheStatusInCapitals()
    {
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("code-274", "label-138");
        var view = TieredLicenseSeatRuleView.ToTieredLicenseSeatRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredLicenseSeatRuleView.ToTieredLicenseSeatRuleViews(new List<TieredLicenseSeatRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredLicenseSeatRuleViewShowsTheStatusInCapitals();
    }
}

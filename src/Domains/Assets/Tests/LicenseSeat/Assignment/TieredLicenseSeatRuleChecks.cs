namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredLicenseSeatRuleChecks
{
    public static void TieredLicenseSeatRuleStartsInOpen()
    {
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("code-274", "label-138");
        Check.Equal(item.Status, TieredLicenseSeatRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLicenseSeatRuleMovesToResolved()
    {
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("code-274", "label-138");
        try
        {
            item.MoveTo(TieredLicenseSeatRuleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLicenseSeatRuleStatus.Resolved, "status");
    }

    public static void TieredLicenseSeatRuleRejectsMovesFromArchived()
    {
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("code-274", "label-138");
        item.Status = TieredLicenseSeatRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredLicenseSeatRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredLicenseSeatRuleStartsInOpen();
        TieredLicenseSeatRuleMovesToResolved();
        TieredLicenseSeatRuleRejectsMovesFromArchived();
    }
}

namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class PayBandRuleChecks
{
    public static void PayBandRuleStartsInNew()
    {
        var item = PayBandRule.NewPayBandRule("code-811", "label-612");
        Check.Equal(item.Status, PayBandRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandRuleMovesToDone()
    {
        var item = PayBandRule.NewPayBandRule("code-811", "label-612");
        try
        {
            item.MoveTo(PayBandRuleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandRuleStatus.Done, "status");
    }

    public static void PayBandRuleRejectsMovesFromFailed()
    {
        var item = PayBandRule.NewPayBandRule("code-811", "label-612");
        item.Status = PayBandRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(PayBandRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        PayBandRuleStartsInNew();
        PayBandRuleMovesToDone();
        PayBandRuleRejectsMovesFromFailed();
    }
}

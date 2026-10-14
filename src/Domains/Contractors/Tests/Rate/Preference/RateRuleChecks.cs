namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RateRuleChecks
{
    public static void RateRuleStartsInNew()
    {
        var item = RateRule.NewRateRule("code-87", "name-441");
        Check.Equal(item.Status, RateRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateRuleMovesToQueued()
    {
        var item = RateRule.NewRateRule("code-87", "name-441");
        try
        {
            item.MoveTo(RateRuleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateRuleStatus.Queued, "status");
    }

    public static void RateRuleRejectsMovesFromFailed()
    {
        var item = RateRule.NewRateRule("code-87", "name-441");
        item.Status = RateRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(RateRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        RateRuleStartsInNew();
        RateRuleMovesToQueued();
        RateRuleRejectsMovesFromFailed();
    }
}

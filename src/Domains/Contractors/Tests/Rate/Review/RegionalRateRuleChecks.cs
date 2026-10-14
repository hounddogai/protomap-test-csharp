namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalRateRuleChecks
{
    public static void RegionalRateRuleStartsInPlanned()
    {
        var item = RegionalRateRule.NewRegionalRateRule("code-372", "title-729");
        Check.Equal(item.Status, RegionalRateRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalRateRuleMovesToScheduled()
    {
        var item = RegionalRateRule.NewRegionalRateRule("code-372", "title-729");
        try
        {
            item.MoveTo(RegionalRateRuleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalRateRuleStatus.Scheduled, "status");
    }

    public static void RegionalRateRuleRejectsMovesFromCancelled()
    {
        var item = RegionalRateRule.NewRegionalRateRule("code-372", "title-729");
        item.Status = RegionalRateRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalRateRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalRateRuleStartsInPlanned();
        RegionalRateRuleMovesToScheduled();
        RegionalRateRuleRejectsMovesFromCancelled();
    }
}

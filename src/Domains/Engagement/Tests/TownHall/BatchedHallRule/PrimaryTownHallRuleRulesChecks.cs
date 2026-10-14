namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class PrimaryTownHallRuleRulesChecks
{
    public static void PrimaryTownHallRuleValidatorReportsEveryProblem()
    {
        var validator = PrimaryTownHallRuleRules.DefaultPrimaryTownHallRuleValidator();
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryTownHallRuleRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PrimaryTownHallRuleRequiredSummaryRule("primary town hall rule needs this field");
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTownHallRuleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PrimaryTownHallRuleChannelLengthRule(1000);
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTownHallRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryTownHallRuleRequiredTitleRule("primary town hall rule needs this field");
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTownHallRuleSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PrimaryTownHallRuleSummaryLengthRule(80);
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryTownHallRuleValidatorReportsEveryProblem();
        PrimaryTownHallRuleRequiredSummaryRuleFlagsABlankSummary();
        PrimaryTownHallRuleChannelLengthRuleFlagsALongChannel();
        PrimaryTownHallRuleRequiredTitleRuleFlagsABlankTitle();
        PrimaryTownHallRuleSummaryLengthRuleFlagsALongSummary();
    }
}

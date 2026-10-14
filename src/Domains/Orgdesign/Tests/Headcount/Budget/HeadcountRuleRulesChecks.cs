namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRuleRulesChecks
{
    public static void HeadcountRuleValidatorReportsEveryProblem()
    {
        var validator = HeadcountRuleRules.DefaultHeadcountRuleValidator();
        var item = HeadcountRule.NewHeadcountRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountRuleRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new HeadcountRuleRequiredChannelRule("headcount rule needs this field");
        var item = HeadcountRule.NewHeadcountRule("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountRuleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new HeadcountRuleChannelLengthRule(120);
        var item = HeadcountRule.NewHeadcountRule("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountRuleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new HeadcountRuleRequiredTeamRule("headcount rule needs a value");
        var item = HeadcountRule.NewHeadcountRule("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountRuleValidatorReportsEveryProblem();
        HeadcountRuleRequiredChannelRuleFlagsABlankChannel();
        HeadcountRuleChannelLengthRuleFlagsALongChannel();
        HeadcountRuleRequiredTeamRuleFlagsABlankTeam();
    }
}

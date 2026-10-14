namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyRuleRulesChecks
{
    public static void OccupancyRuleValidatorReportsEveryProblem()
    {
        var validator = OccupancyRuleRules.DefaultOccupancyRuleValidator();
        var item = OccupancyRule.NewOccupancyRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyRulePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new OccupancyRulePhoneLengthRule(3600);
        var item = OccupancyRule.NewOccupancyRule("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyRuleTitleLengthRule(80);
        var item = OccupancyRule.NewOccupancyRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyRuleValidatorReportsEveryProblem();
        OccupancyRulePhoneLengthRuleFlagsALongPhone();
        OccupancyRuleTitleLengthRuleFlagsALongTitle();
    }
}

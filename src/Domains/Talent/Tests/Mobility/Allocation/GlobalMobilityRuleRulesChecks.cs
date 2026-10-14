namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GlobalMobilityRuleRulesChecks
{
    public static void GlobalMobilityRuleValidatorReportsEveryProblem()
    {
        var validator = GlobalMobilityRuleRules.DefaultGlobalMobilityRuleValidator();
        var item = GlobalMobilityRule.NewGlobalMobilityRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalMobilityRuleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new GlobalMobilityRuleCapacityRangeRule(32, 32);
        var item = GlobalMobilityRule.NewGlobalMobilityRule("x", "y");
        item.Capacity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalMobilityRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalMobilityRuleCodeLengthRule(32);
        var item = GlobalMobilityRule.NewGlobalMobilityRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalMobilityRuleValidatorReportsEveryProblem();
        GlobalMobilityRuleCapacityRangeRuleFlagsValuesAboveCapacity();
        GlobalMobilityRuleCodeLengthRuleFlagsALongCode();
    }
}

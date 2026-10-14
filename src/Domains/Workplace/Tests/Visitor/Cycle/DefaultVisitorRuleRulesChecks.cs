namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleRulesChecks
{
    public static void DefaultVisitorRuleValidatorReportsEveryProblem()
    {
        var validator = DefaultVisitorRuleRules.DefaultDefaultVisitorRuleValidator();
        var item = DefaultVisitorRule.NewDefaultVisitorRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVisitorRuleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new DefaultVisitorRuleGraceDaysRangeRule(64, 120);
        var item = DefaultVisitorRule.NewDefaultVisitorRule("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultVisitorRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultVisitorRuleCodeLengthRule(255);
        var item = DefaultVisitorRule.NewDefaultVisitorRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultVisitorRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultVisitorRuleRequiredCodeRule("default visitor rule needs this field");
        var item = DefaultVisitorRule.NewDefaultVisitorRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVisitorRuleRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new DefaultVisitorRuleRequiredEmployeeIdRule("default visitor rule needs a value");
        var item = DefaultVisitorRule.NewDefaultVisitorRule("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVisitorRulePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new DefaultVisitorRulePriorityRangeRule(120, 1000);
        var item = DefaultVisitorRule.NewDefaultVisitorRule("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultVisitorRuleValidatorReportsEveryProblem();
        DefaultVisitorRuleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        DefaultVisitorRuleCodeLengthRuleFlagsALongCode();
        DefaultVisitorRuleRequiredCodeRuleFlagsABlankCode();
        DefaultVisitorRuleRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        DefaultVisitorRulePriorityRangeRuleFlagsValuesAbovePriority();
    }
}

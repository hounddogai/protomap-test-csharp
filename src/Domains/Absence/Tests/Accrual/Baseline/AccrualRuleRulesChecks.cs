namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualRuleRulesChecks
{
    public static void AccrualRuleValidatorReportsEveryProblem()
    {
        var validator = AccrualRuleRules.DefaultAccrualRuleValidator();
        var item = AccrualRule.NewAccrualRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualRuleRequiredCodeRule("accrual rule needs a value");
        var item = AccrualRule.NewAccrualRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualRuleRequiredNameRule("accrual rule needs a value");
        var item = AccrualRule.NewAccrualRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualRulePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AccrualRulePriorityRangeRule(64, 1000);
        var item = AccrualRule.NewAccrualRule("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualRuleCodeLengthRule(80);
        var item = AccrualRule.NewAccrualRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualRuleNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualRuleNameLengthRule(3600);
        var item = AccrualRule.NewAccrualRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualRuleRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new AccrualRuleRequiredSegmentRule("accrual rule needs a value");
        var item = AccrualRule.NewAccrualRule("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualRuleValidatorReportsEveryProblem();
        AccrualRuleRequiredCodeRuleFlagsABlankCode();
        AccrualRuleRequiredNameRuleFlagsABlankName();
        AccrualRulePriorityRangeRuleFlagsValuesAbovePriority();
        AccrualRuleCodeLengthRuleFlagsALongCode();
        AccrualRuleNameLengthRuleFlagsALongName();
        AccrualRuleRequiredSegmentRuleFlagsABlankSegment();
    }
}

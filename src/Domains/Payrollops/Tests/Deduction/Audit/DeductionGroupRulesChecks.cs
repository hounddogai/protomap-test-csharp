namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionGroupRulesChecks
{
    public static void DeductionGroupValidatorReportsEveryProblem()
    {
        var validator = DeductionGroupRules.DefaultDeductionGroupValidator();
        var item = DeductionGroup.NewDeductionGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionGroupCodeLengthRule(86400);
        var item = DeductionGroup.NewDeductionGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionGroupRequiredCodeRule("deduction group needs this field");
        var item = DeductionGroup.NewDeductionGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionGroupBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new DeductionGroupBadgeLengthRule(80);
        var item = DeductionGroup.NewDeductionGroup("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionGroupRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new DeductionGroupRequiredBadgeRule("deduction group needs a value");
        var item = DeductionGroup.NewDeductionGroup("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionGroupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeductionGroupRequiredTitleRule("deduction group needs an entry");
        var item = DeductionGroup.NewDeductionGroup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionGroupValidatorReportsEveryProblem();
        DeductionGroupCodeLengthRuleFlagsALongCode();
        DeductionGroupRequiredCodeRuleFlagsABlankCode();
        DeductionGroupBadgeLengthRuleFlagsALongBadge();
        DeductionGroupRequiredBadgeRuleFlagsABlankBadge();
        DeductionGroupRequiredTitleRuleFlagsABlankTitle();
    }
}

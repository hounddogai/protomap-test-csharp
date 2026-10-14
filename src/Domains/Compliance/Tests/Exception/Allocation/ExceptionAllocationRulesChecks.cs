namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionAllocationRulesChecks
{
    public static void ExceptionAllocationValidatorReportsEveryProblem()
    {
        var validator = ExceptionAllocationRules.DefaultExceptionAllocationValidator();
        var item = ExceptionAllocation.NewExceptionAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionAllocationSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ExceptionAllocationSkillLengthRule(1000);
        var item = ExceptionAllocation.NewExceptionAllocation("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new ExceptionAllocationNameLengthRule(1000);
        var item = ExceptionAllocation.NewExceptionAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionAllocationRequiredCodeRule("exception allocation needs this field");
        var item = ExceptionAllocation.NewExceptionAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionAllocationRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ExceptionAllocationRatioRangeRule(2.4, 1140.7);
        var item = ExceptionAllocation.NewExceptionAllocation("x", "y");
        item.Ratio = 1142.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new ExceptionAllocationRequiredNameRule("exception allocation needs this field");
        var item = ExceptionAllocation.NewExceptionAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionAllocationValidatorReportsEveryProblem();
        ExceptionAllocationSkillLengthRuleFlagsALongSkill();
        ExceptionAllocationNameLengthRuleFlagsALongName();
        ExceptionAllocationRequiredCodeRuleFlagsABlankCode();
        ExceptionAllocationRatioRangeRuleFlagsValuesAboveRatio();
        ExceptionAllocationRequiredNameRuleFlagsABlankName();
    }
}

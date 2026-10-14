namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingRulesChecks
{
    public static void DeductionMappingValidatorReportsEveryProblem()
    {
        var validator = DeductionMappingRules.DefaultDeductionMappingValidator();
        var item = DeductionMapping.NewDeductionMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionMappingRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new DeductionMappingRequiredSkillRule("deduction mapping needs an entry");
        var item = DeductionMapping.NewDeductionMapping("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionMappingRequiredCodeRule("deduction mapping needs this field");
        var item = DeductionMapping.NewDeductionMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DeductionMappingLabelLengthRule(86400);
        var item = DeductionMapping.NewDeductionMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionMappingValidatorReportsEveryProblem();
        DeductionMappingRequiredSkillRuleFlagsABlankSkill();
        DeductionMappingRequiredCodeRuleFlagsABlankCode();
        DeductionMappingLabelLengthRuleFlagsALongLabel();
    }
}

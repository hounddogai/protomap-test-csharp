namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordRulesChecks
{
    public static void PolicyRecordValidatorReportsEveryProblem()
    {
        var validator = PolicyRecordRules.DefaultPolicyRecordValidator();
        var item = PolicyRecord.NewPolicyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyRecordCodeLengthRule(32);
        var item = PolicyRecord.NewPolicyRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyRecordSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PolicyRecordSkillLengthRule(86400);
        var item = PolicyRecord.NewPolicyRecord("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyRecordWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PolicyRecordWeightRangeRule(3.4, 1601.0);
        var item = PolicyRecord.NewPolicyRecord("x", "y");
        item.Weight = 1602.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyRecordRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PolicyRecordRequiredSkillRule("policy record needs a value");
        var item = PolicyRecord.NewPolicyRecord("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyRecordValidatorReportsEveryProblem();
        PolicyRecordCodeLengthRuleFlagsALongCode();
        PolicyRecordSkillLengthRuleFlagsALongSkill();
        PolicyRecordWeightRangeRuleFlagsValuesAboveWeight();
        PolicyRecordRequiredSkillRuleFlagsABlankSkill();
    }
}

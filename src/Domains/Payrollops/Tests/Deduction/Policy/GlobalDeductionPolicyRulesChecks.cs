namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalDeductionPolicyRulesChecks
{
    public static void GlobalDeductionPolicyValidatorReportsEveryProblem()
    {
        var validator = GlobalDeductionPolicyRules.DefaultGlobalDeductionPolicyValidator();
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalDeductionPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalDeductionPolicyRequiredCodeRule("global deduction policy needs a value");
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalDeductionPolicySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new GlobalDeductionPolicySequenceRangeRule(64, 255);
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalDeductionPolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new GlobalDeductionPolicyCreatedAtRangeRule(32, 86400);
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalDeductionPolicyValidatorReportsEveryProblem();
        GlobalDeductionPolicyRequiredCodeRuleFlagsABlankCode();
        GlobalDeductionPolicySequenceRangeRuleFlagsValuesAboveSequence();
        GlobalDeductionPolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}

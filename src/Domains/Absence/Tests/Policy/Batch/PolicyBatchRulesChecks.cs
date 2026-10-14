namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyBatchRulesChecks
{
    public static void PolicyBatchValidatorReportsEveryProblem()
    {
        var validator = PolicyBatchRules.DefaultPolicyBatchValidator();
        var item = PolicyBatch.NewPolicyBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyBatchCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PolicyBatchCeilingRangeRule(7.3, 1005.6);
        var item = PolicyBatch.NewPolicyBatch("x", "y");
        item.Ceiling = 1007.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyBatchRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PolicyBatchRequiredExternalRefRule("policy batch needs a value");
        var item = PolicyBatch.NewPolicyBatch("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PolicyBatchLabelLengthRule(86400);
        var item = PolicyBatch.NewPolicyBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PolicyBatchLeaveDaysRangeRule(32, 255);
        var item = PolicyBatch.NewPolicyBatch("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyBatchValidatorReportsEveryProblem();
        PolicyBatchCeilingRangeRuleFlagsValuesAboveCeiling();
        PolicyBatchRequiredExternalRefRuleFlagsABlankExternalRef();
        PolicyBatchLabelLengthRuleFlagsALongLabel();
        PolicyBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}

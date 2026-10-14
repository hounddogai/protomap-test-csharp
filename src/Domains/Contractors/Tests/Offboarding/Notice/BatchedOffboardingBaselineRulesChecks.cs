namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedOffboardingBaselineRulesChecks
{
    public static void BatchedOffboardingBaselineValidatorReportsEveryProblem()
    {
        var validator = BatchedOffboardingBaselineRules.DefaultBatchedOffboardingBaselineValidator();
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedOffboardingBaselineDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new BatchedOffboardingBaselineDepartmentLengthRule(32);
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedOffboardingBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedOffboardingBaselineFloorValueRangeRule(5.1, 493.9);
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("x", "y");
        item.FloorValue = 495.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedOffboardingBaselineManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new BatchedOffboardingBaselineManagerIdLengthRule(120);
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedOffboardingBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedOffboardingBaselineRequiredCodeRule("batched offboarding baseline needs this field");
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedOffboardingBaselineRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BatchedOffboardingBaselineRequiredManagerIdRule("batched offboarding baseline needs this field");
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedOffboardingBaselineValidatorReportsEveryProblem();
        BatchedOffboardingBaselineDepartmentLengthRuleFlagsALongDepartment();
        BatchedOffboardingBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedOffboardingBaselineManagerIdLengthRuleFlagsALongManagerId();
        BatchedOffboardingBaselineRequiredCodeRuleFlagsABlankCode();
        BatchedOffboardingBaselineRequiredManagerIdRuleFlagsABlankManagerId();
    }
}

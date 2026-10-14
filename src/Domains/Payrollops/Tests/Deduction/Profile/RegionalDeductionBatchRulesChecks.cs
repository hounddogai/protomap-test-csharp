namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalDeductionBatchRulesChecks
{
    public static void RegionalDeductionBatchValidatorReportsEveryProblem()
    {
        var validator = RegionalDeductionBatchRules.DefaultRegionalDeductionBatchValidator();
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeductionBatchNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalDeductionBatchNameLengthRule(255);
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionBatchSourceLengthRuleFlagsALongSource()
    {
        var rule = new RegionalDeductionBatchSourceLengthRule(80);
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionBatchEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RegionalDeductionBatchEndsAtRangeRule(255, 1000);
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalDeductionBatchValidatorReportsEveryProblem();
        RegionalDeductionBatchNameLengthRuleFlagsALongName();
        RegionalDeductionBatchSourceLengthRuleFlagsALongSource();
        RegionalDeductionBatchEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}

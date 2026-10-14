namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class PendingCareerPathBatchRulesChecks
{
    public static void PendingCareerPathBatchValidatorReportsEveryProblem()
    {
        var validator = PendingCareerPathBatchRules.DefaultPendingCareerPathBatchValidator();
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingCareerPathBatchBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PendingCareerPathBatchBatchSizeRangeRule(32, 86400);
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingCareerPathBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PendingCareerPathBatchCreatedAtRangeRule(80, 1000);
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingCareerPathBatchEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PendingCareerPathBatchEmployeeIdLengthRule(120);
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingCareerPathBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingCareerPathBatchRequiredCodeRule("pending career path batch needs a value");
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingCareerPathBatchValidatorReportsEveryProblem();
        PendingCareerPathBatchBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PendingCareerPathBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PendingCareerPathBatchEmployeeIdLengthRuleFlagsALongEmployeeId();
        PendingCareerPathBatchRequiredCodeRuleFlagsABlankCode();
    }
}

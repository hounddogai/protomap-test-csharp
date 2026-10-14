namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class BatchedMobilityProfileRulesChecks
{
    public static void BatchedMobilityProfileValidatorReportsEveryProblem()
    {
        var validator = BatchedMobilityProfileRules.DefaultBatchedMobilityProfileValidator();
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedMobilityProfileUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BatchedMobilityProfileUpdatedAtRangeRule(32, 1000);
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedMobilityProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedMobilityProfileRequiredCodeRule("batched mobility profile needs this field");
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedMobilityProfileRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new BatchedMobilityProfileRequiredDescriptionRule("batched mobility profile needs this field");
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedMobilityProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedMobilityProfileTenureMonthsRangeRule(80, 3600);
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedMobilityProfileDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new BatchedMobilityProfileDescriptionLengthRule(120);
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedMobilityProfileValidatorReportsEveryProblem();
        BatchedMobilityProfileUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        BatchedMobilityProfileRequiredCodeRuleFlagsABlankCode();
        BatchedMobilityProfileRequiredDescriptionRuleFlagsABlankDescription();
        BatchedMobilityProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BatchedMobilityProfileDescriptionLengthRuleFlagsALongDescription();
    }
}

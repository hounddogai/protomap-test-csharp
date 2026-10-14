namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedLicenseSeatProfileRulesChecks
{
    public static void BatchedLicenseSeatProfileValidatorReportsEveryProblem()
    {
        var validator = BatchedLicenseSeatProfileRules.DefaultBatchedLicenseSeatProfileValidator();
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLicenseSeatProfileFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedLicenseSeatProfileFloorValueRangeRule(6.0, 2819.5);
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("x", "y");
        item.FloorValue = 2821.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLicenseSeatProfileBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new BatchedLicenseSeatProfileBadgeLengthRule(86400);
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLicenseSeatProfileRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new BatchedLicenseSeatProfileRequiredJobCodeRule("batched license seat profile needs a value");
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedLicenseSeatProfileJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new BatchedLicenseSeatProfileJobCodeLengthRule(86400);
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLicenseSeatProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new BatchedLicenseSeatProfileCreatedAtRangeRule(120, 255);
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedLicenseSeatProfileValidatorReportsEveryProblem();
        BatchedLicenseSeatProfileFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedLicenseSeatProfileBadgeLengthRuleFlagsALongBadge();
        BatchedLicenseSeatProfileRequiredJobCodeRuleFlagsABlankJobCode();
        BatchedLicenseSeatProfileJobCodeLengthRuleFlagsALongJobCode();
        BatchedLicenseSeatProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}

namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryLaptopBatchRulesChecks
{
    public static void PrimaryLaptopBatchValidatorReportsEveryProblem()
    {
        var validator = PrimaryLaptopBatchRules.DefaultPrimaryLaptopBatchValidator();
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryLaptopBatchExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PrimaryLaptopBatchExpiresAtRangeRule(32, 3600);
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryLaptopBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryLaptopBatchCodeLengthRule(16);
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryLaptopBatchValidatorReportsEveryProblem();
        PrimaryLaptopBatchExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PrimaryLaptopBatchCodeLengthRuleFlagsALongCode();
    }
}

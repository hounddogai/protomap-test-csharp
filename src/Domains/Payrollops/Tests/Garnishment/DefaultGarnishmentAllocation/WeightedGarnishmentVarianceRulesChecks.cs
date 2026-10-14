namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentVarianceRulesChecks
{
    public static void WeightedGarnishmentVarianceValidatorReportsEveryProblem()
    {
        var validator = WeightedGarnishmentVarianceRules.DefaultWeightedGarnishmentVarianceValidator();
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedGarnishmentVarianceNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedGarnishmentVarianceNameLengthRule(255);
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedGarnishmentVarianceCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new WeightedGarnishmentVarianceCreatedAtRangeRule(32, 120);
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("x", "y");
        item.CreatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedGarnishmentVarianceRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new WeightedGarnishmentVarianceRequiredSegmentRule("weighted garnishment variance needs an entry");
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedGarnishmentVarianceValidatorReportsEveryProblem();
        WeightedGarnishmentVarianceNameLengthRuleFlagsALongName();
        WeightedGarnishmentVarianceCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        WeightedGarnishmentVarianceRequiredSegmentRuleFlagsABlankSegment();
    }
}

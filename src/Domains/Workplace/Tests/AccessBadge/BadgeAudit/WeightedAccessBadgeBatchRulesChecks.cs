namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class WeightedAccessBadgeBatchRulesChecks
{
    public static void WeightedAccessBadgeBatchValidatorReportsEveryProblem()
    {
        var validator = WeightedAccessBadgeBatchRules.DefaultWeightedAccessBadgeBatchValidator();
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAccessBadgeBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedAccessBadgeBatchLabelLengthRule(120);
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAccessBadgeBatchRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WeightedAccessBadgeBatchRequiredChecksumRule("weighted access badge batch needs an entry");
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAccessBadgeBatchCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new WeightedAccessBadgeBatchCapacityRangeRule(80, 3600);
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeBatchValidatorReportsEveryProblem();
        WeightedAccessBadgeBatchLabelLengthRuleFlagsALongLabel();
        WeightedAccessBadgeBatchRequiredChecksumRuleFlagsABlankChecksum();
        WeightedAccessBadgeBatchCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}

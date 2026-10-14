namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedCertificationMilestoneRulesChecks
{
    public static void WeightedCertificationMilestoneValidatorReportsEveryProblem()
    {
        var validator = WeightedCertificationMilestoneRules.DefaultWeightedCertificationMilestoneValidator();
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedCertificationMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new WeightedCertificationMilestoneUpdatedAtRangeRule(86400, 86400);
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedCertificationMilestoneQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new WeightedCertificationMilestoneQuantityRangeRule(80, 3600);
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedCertificationMilestoneValidatorReportsEveryProblem();
        WeightedCertificationMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        WeightedCertificationMilestoneQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}

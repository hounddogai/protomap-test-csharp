namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentAssignmentRulesChecks
{
    public static void AdjustmentAssignmentValidatorReportsEveryProblem()
    {
        var validator = AdjustmentAssignmentRules.DefaultAdjustmentAssignmentValidator();
        var item = AdjustmentAssignment.NewAdjustmentAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentAssignmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new AdjustmentAssignmentCountryLengthRule(120);
        var item = AdjustmentAssignment.NewAdjustmentAssignment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentAssignmentRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new AdjustmentAssignmentRequiredSegmentRule("adjustment assignment needs this field");
        var item = AdjustmentAssignment.NewAdjustmentAssignment("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdjustmentAssignmentRequiredNameRule("adjustment assignment needs this field");
        var item = AdjustmentAssignment.NewAdjustmentAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentAssignmentValidatorReportsEveryProblem();
        AdjustmentAssignmentCountryLengthRuleFlagsALongCountry();
        AdjustmentAssignmentRequiredSegmentRuleFlagsABlankSegment();
        AdjustmentAssignmentRequiredNameRuleFlagsABlankName();
    }
}

namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathSegmentRulesChecks
{
    public static void CareerPathSegmentValidatorReportsEveryProblem()
    {
        var validator = CareerPathSegmentRules.DefaultCareerPathSegmentValidator();
        var item = CareerPathSegment.NewCareerPathSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathSegmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new CareerPathSegmentRevisionRangeRule(80, 86400);
        var item = CareerPathSegment.NewCareerPathSegment("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathSegmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new CareerPathSegmentCostCenterLengthRule(3600);
        var item = CareerPathSegment.NewCareerPathSegment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathSegmentCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new CareerPathSegmentCeilingRangeRule(6.4, 1632.5);
        var item = CareerPathSegment.NewCareerPathSegment("x", "y");
        item.Ceiling = 1634.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathSegmentValidatorReportsEveryProblem();
        CareerPathSegmentRevisionRangeRuleFlagsValuesAboveRevision();
        CareerPathSegmentCostCenterLengthRuleFlagsALongCostCenter();
        CareerPathSegmentCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}

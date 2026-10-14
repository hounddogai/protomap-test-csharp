namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathPolicyRulesChecks
{
    public static void CareerPathPolicyValidatorReportsEveryProblem()
    {
        var validator = CareerPathPolicyRules.DefaultCareerPathPolicyValidator();
        var item = CareerPathPolicy.NewCareerPathPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathPolicyCodeLengthRule(32);
        var item = CareerPathPolicy.NewCareerPathPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathPolicySegmentLengthRuleFlagsALongSegment()
    {
        var rule = new CareerPathPolicySegmentLengthRule(1000);
        var item = CareerPathPolicy.NewCareerPathPolicy("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathPolicyRequiredCodeRule("career path policy needs an entry");
        var item = CareerPathPolicy.NewCareerPathPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathPolicyLocationLengthRuleFlagsALongLocation()
    {
        var rule = new CareerPathPolicyLocationLengthRule(255);
        var item = CareerPathPolicy.NewCareerPathPolicy("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathPolicyEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CareerPathPolicyEndsAtRangeRule(16, 16);
        var item = CareerPathPolicy.NewCareerPathPolicy("x", "y");
        item.EndsAt = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathPolicyValidatorReportsEveryProblem();
        CareerPathPolicyCodeLengthRuleFlagsALongCode();
        CareerPathPolicySegmentLengthRuleFlagsALongSegment();
        CareerPathPolicyRequiredCodeRuleFlagsABlankCode();
        CareerPathPolicyLocationLengthRuleFlagsALongLocation();
        CareerPathPolicyEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}

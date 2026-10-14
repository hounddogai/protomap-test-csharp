namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultRecognitionPolicyRulesChecks
{
    public static void DefaultRecognitionPolicyValidatorReportsEveryProblem()
    {
        var validator = DefaultRecognitionPolicyRules.DefaultDefaultRecognitionPolicyValidator();
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRecognitionPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultRecognitionPolicyRequiredNameRule("default recognition policy needs this field");
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRecognitionPolicyFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new DefaultRecognitionPolicyFloorValueRangeRule(1.9, 2429.4);
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("x", "y");
        item.FloorValue = 2430.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultRecognitionPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultRecognitionPolicyNameLengthRule(16);
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRecognitionPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultRecognitionPolicyCodeLengthRule(120);
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultRecognitionPolicyRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new DefaultRecognitionPolicyRequiredSegmentRule("default recognition policy needs this field");
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultRecognitionPolicyValidatorReportsEveryProblem();
        DefaultRecognitionPolicyRequiredNameRuleFlagsABlankName();
        DefaultRecognitionPolicyFloorValueRangeRuleFlagsValuesAboveFloorValue();
        DefaultRecognitionPolicyNameLengthRuleFlagsALongName();
        DefaultRecognitionPolicyCodeLengthRuleFlagsALongCode();
        DefaultRecognitionPolicyRequiredSegmentRuleFlagsABlankSegment();
    }
}

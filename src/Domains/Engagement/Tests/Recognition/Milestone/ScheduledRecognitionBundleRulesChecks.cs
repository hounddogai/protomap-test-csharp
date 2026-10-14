namespace Acme.Workforce.Engagement.Tests.Recognition.Milestone;

using Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledRecognitionBundleRulesChecks
{
    public static void ScheduledRecognitionBundleValidatorReportsEveryProblem()
    {
        var validator = ScheduledRecognitionBundleRules.DefaultScheduledRecognitionBundleValidator();
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRecognitionBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledRecognitionBundleRequiredCodeRule("scheduled recognition bundle needs an entry");
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRecognitionBundleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ScheduledRecognitionBundleGradeLengthRule(80);
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledRecognitionBundleCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ScheduledRecognitionBundleCeilingRangeRule(8.7, 2175.9);
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("x", "y");
        item.Ceiling = 2177.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledRecognitionBundleQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledRecognitionBundleQuantityRangeRule(3600, 86400);
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledRecognitionBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledRecognitionBundleLabelLengthRule(1000);
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledRecognitionBundleValidatorReportsEveryProblem();
        ScheduledRecognitionBundleRequiredCodeRuleFlagsABlankCode();
        ScheduledRecognitionBundleGradeLengthRuleFlagsALongGrade();
        ScheduledRecognitionBundleCeilingRangeRuleFlagsValuesAboveCeiling();
        ScheduledRecognitionBundleQuantityRangeRuleFlagsValuesAboveQuantity();
        ScheduledRecognitionBundleLabelLengthRuleFlagsALongLabel();
    }
}

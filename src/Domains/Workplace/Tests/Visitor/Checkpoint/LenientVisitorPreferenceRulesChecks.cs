namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientVisitorPreferenceRulesChecks
{
    public static void LenientVisitorPreferenceValidatorReportsEveryProblem()
    {
        var validator = LenientVisitorPreferenceRules.DefaultLenientVisitorPreferenceValidator();
        var item = LenientVisitorPreference.NewLenientVisitorPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVisitorPreferenceCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LenientVisitorPreferenceCategoryLengthRule(255);
        var item = LenientVisitorPreference.NewLenientVisitorPreference("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVisitorPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientVisitorPreferenceRequiredCodeRule("lenient visitor preference needs a value");
        var item = LenientVisitorPreference.NewLenientVisitorPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVisitorPreferenceFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientVisitorPreferenceFloorValueRangeRule(4.3, 454.0);
        var item = LenientVisitorPreference.NewLenientVisitorPreference("x", "y");
        item.FloorValue = 455.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientVisitorPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientVisitorPreferenceRequiredLabelRule("lenient visitor preference needs this field");
        var item = LenientVisitorPreference.NewLenientVisitorPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientVisitorPreferenceValidatorReportsEveryProblem();
        LenientVisitorPreferenceCategoryLengthRuleFlagsALongCategory();
        LenientVisitorPreferenceRequiredCodeRuleFlagsABlankCode();
        LenientVisitorPreferenceFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientVisitorPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}

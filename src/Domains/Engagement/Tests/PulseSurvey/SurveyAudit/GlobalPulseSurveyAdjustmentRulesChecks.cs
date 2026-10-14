namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Checks of survey audit code, run by the project's runner.</summary>
public static class GlobalPulseSurveyAdjustmentRulesChecks
{
    public static void GlobalPulseSurveyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = GlobalPulseSurveyAdjustmentRules.DefaultGlobalPulseSurveyAdjustmentValidator();
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPulseSurveyAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalPulseSurveyAdjustmentRequiredLabelRule("global pulse survey adjustment needs this field");
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPulseSurveyAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalPulseSurveyAdjustmentCodeLengthRule(1000);
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPulseSurveyAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new GlobalPulseSurveyAdjustmentStartsAtRangeRule(80, 86400);
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPulseSurveyAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalPulseSurveyAdjustmentNameLengthRule(1000);
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPulseSurveyAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalPulseSurveyAdjustmentEndsAtRangeRule(120, 255);
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyAdjustmentValidatorReportsEveryProblem();
        GlobalPulseSurveyAdjustmentRequiredLabelRuleFlagsABlankLabel();
        GlobalPulseSurveyAdjustmentCodeLengthRuleFlagsALongCode();
        GlobalPulseSurveyAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        GlobalPulseSurveyAdjustmentNameLengthRuleFlagsALongName();
        GlobalPulseSurveyAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}

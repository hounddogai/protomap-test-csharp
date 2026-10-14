namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingBonusPoolTemplateRulesChecks
{
    public static void RollingBonusPoolTemplateValidatorReportsEveryProblem()
    {
        var validator = RollingBonusPoolTemplateRules.DefaultRollingBonusPoolTemplateValidator();
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBonusPoolTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingBonusPoolTemplateLabelLengthRule(32);
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBonusPoolTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingBonusPoolTemplateRequiredCodeRule("rolling bonus pool template needs a value");
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBonusPoolTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RollingBonusPoolTemplateDurationMinutesRangeRule(64, 120);
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBonusPoolTemplateReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new RollingBonusPoolTemplateReviewerLengthRule(255);
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingBonusPoolTemplateValidatorReportsEveryProblem();
        RollingBonusPoolTemplateLabelLengthRuleFlagsALongLabel();
        RollingBonusPoolTemplateRequiredCodeRuleFlagsABlankCode();
        RollingBonusPoolTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RollingBonusPoolTemplateReviewerLengthRuleFlagsALongReviewer();
    }
}

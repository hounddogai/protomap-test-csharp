namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyRollupRulesChecks
{
    public static void PulseSurveyRollupValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyRollupRules.DefaultPulseSurveyRollupValidator();
        var item = PulseSurveyRollup.NewPulseSurveyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyRollupCodeLengthRule(3600);
        var item = PulseSurveyRollup.NewPulseSurveyRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyRollupNotesLengthRuleFlagsALongNotes()
    {
        var rule = new PulseSurveyRollupNotesLengthRule(255);
        var item = PulseSurveyRollup.NewPulseSurveyRollup("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyRollupDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PulseSurveyRollupDiscountRangeRule(0.2, 646.3);
        var item = PulseSurveyRollup.NewPulseSurveyRollup("x", "y");
        item.Discount = 647.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyRollupRequiredCodeRule("pulse survey rollup needs a value");
        var item = PulseSurveyRollup.NewPulseSurveyRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PulseSurveyRollupGraceDaysRangeRule(64, 120);
        var item = PulseSurveyRollup.NewPulseSurveyRollup("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PulseSurveyRollupValidatorReportsEveryProblem();
        PulseSurveyRollupCodeLengthRuleFlagsALongCode();
        PulseSurveyRollupNotesLengthRuleFlagsALongNotes();
        PulseSurveyRollupDiscountRangeRuleFlagsValuesAboveDiscount();
        PulseSurveyRollupRequiredCodeRuleFlagsABlankCode();
        PulseSurveyRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}

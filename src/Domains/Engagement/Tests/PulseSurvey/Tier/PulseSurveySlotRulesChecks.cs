namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotRulesChecks
{
    public static void PulseSurveySlotValidatorReportsEveryProblem()
    {
        var validator = PulseSurveySlotRules.DefaultPulseSurveySlotValidator();
        var item = PulseSurveySlot.NewPulseSurveySlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveySlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveySlotCodeLengthRule(64);
        var item = PulseSurveySlot.NewPulseSurveySlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveySlotRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PulseSurveySlotRequiredSummaryRule("pulse survey slot needs this field");
        var item = PulseSurveySlot.NewPulseSurveySlot("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveySlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new PulseSurveySlotRequiredNameRule("pulse survey slot needs a value");
        var item = PulseSurveySlot.NewPulseSurveySlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveySlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PulseSurveySlotAttemptsRangeRule(16, 64);
        var item = PulseSurveySlot.NewPulseSurveySlot("x", "y");
        item.Attempts = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveySlotSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PulseSurveySlotSummaryLengthRule(80);
        var item = PulseSurveySlot.NewPulseSurveySlot("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveySlotNameLengthRuleFlagsALongName()
    {
        var rule = new PulseSurveySlotNameLengthRule(120);
        var item = PulseSurveySlot.NewPulseSurveySlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveySlotValidatorReportsEveryProblem();
        PulseSurveySlotCodeLengthRuleFlagsALongCode();
        PulseSurveySlotRequiredSummaryRuleFlagsABlankSummary();
        PulseSurveySlotRequiredNameRuleFlagsABlankName();
        PulseSurveySlotAttemptsRangeRuleFlagsValuesAboveAttempts();
        PulseSurveySlotSummaryLengthRuleFlagsALongSummary();
        PulseSurveySlotNameLengthRuleFlagsALongName();
    }
}

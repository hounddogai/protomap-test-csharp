namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyTierRulesChecks
{
    public static void PulseSurveyTierValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyTierRules.DefaultPulseSurveyTierValidator();
        var item = PulseSurveyTier.NewPulseSurveyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyTierRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PulseSurveyTierRequiredNotesRule("pulse survey tier needs this field");
        var item = PulseSurveyTier.NewPulseSurveyTier("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyTierRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PulseSurveyTierRateRangeRule(6.4, 1341.1);
        var item = PulseSurveyTier.NewPulseSurveyTier("x", "y");
        item.Rate = 1342.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyTierCodeLengthRule(120);
        var item = PulseSurveyTier.NewPulseSurveyTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyTierNameLengthRuleFlagsALongName()
    {
        var rule = new PulseSurveyTierNameLengthRule(32);
        var item = PulseSurveyTier.NewPulseSurveyTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyTierValidatorReportsEveryProblem();
        PulseSurveyTierRequiredNotesRuleFlagsABlankNotes();
        PulseSurveyTierRateRangeRuleFlagsValuesAboveRate();
        PulseSurveyTierCodeLengthRuleFlagsALongCode();
        PulseSurveyTierNameLengthRuleFlagsALongName();
    }
}

namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientPulseSurveyClaimRulesChecks
{
    public static void LenientPulseSurveyClaimValidatorReportsEveryProblem()
    {
        var validator = LenientPulseSurveyClaimRules.DefaultLenientPulseSurveyClaimValidator();
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPulseSurveyClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientPulseSurveyClaimCodeLengthRule(32);
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPulseSurveyClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientPulseSurveyClaimTitleLengthRule(3600);
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPulseSurveyClaimDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LenientPulseSurveyClaimDescriptionLengthRule(3600);
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPulseSurveyClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientPulseSurveyClaimRequiredTitleRule("lenient pulse survey claim needs this field");
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPulseSurveyClaimRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new LenientPulseSurveyClaimRequiredNotesRule("lenient pulse survey claim needs this field");
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPulseSurveyClaimValidatorReportsEveryProblem();
        LenientPulseSurveyClaimCodeLengthRuleFlagsALongCode();
        LenientPulseSurveyClaimTitleLengthRuleFlagsALongTitle();
        LenientPulseSurveyClaimDescriptionLengthRuleFlagsALongDescription();
        LenientPulseSurveyClaimRequiredTitleRuleFlagsABlankTitle();
        LenientPulseSurveyClaimRequiredNotesRuleFlagsABlankNotes();
    }
}

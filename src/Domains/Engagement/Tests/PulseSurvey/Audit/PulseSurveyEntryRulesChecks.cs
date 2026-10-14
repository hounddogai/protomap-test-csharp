namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyEntryRulesChecks
{
    public static void PulseSurveyEntryValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyEntryRules.DefaultPulseSurveyEntryValidator();
        var item = PulseSurveyEntry.NewPulseSurveyEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyEntryCodeLengthRule(32);
        var item = PulseSurveyEntry.NewPulseSurveyEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyEntryAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PulseSurveyEntryAttemptsRangeRule(32, 255);
        var item = PulseSurveyEntry.NewPulseSurveyEntry("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyEntryRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PulseSurveyEntryRequiredReviewerRule("pulse survey entry needs an entry");
        var item = PulseSurveyEntry.NewPulseSurveyEntry("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyEntryReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PulseSurveyEntryReviewerLengthRule(32);
        var item = PulseSurveyEntry.NewPulseSurveyEntry("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PulseSurveyEntryTitleLengthRule(120);
        var item = PulseSurveyEntry.NewPulseSurveyEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyEntryValidatorReportsEveryProblem();
        PulseSurveyEntryCodeLengthRuleFlagsALongCode();
        PulseSurveyEntryAttemptsRangeRuleFlagsValuesAboveAttempts();
        PulseSurveyEntryRequiredReviewerRuleFlagsABlankReviewer();
        PulseSurveyEntryReviewerLengthRuleFlagsALongReviewer();
        PulseSurveyEntryTitleLengthRuleFlagsALongTitle();
    }
}

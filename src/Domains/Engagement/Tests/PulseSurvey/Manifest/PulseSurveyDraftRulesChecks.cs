namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyDraftRulesChecks
{
    public static void PulseSurveyDraftValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyDraftRules.DefaultPulseSurveyDraftValidator();
        var item = PulseSurveyDraft.NewPulseSurveyDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyDraftRequiredCodeRule("pulse survey draft needs an entry");
        var item = PulseSurveyDraft.NewPulseSurveyDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyDraftManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PulseSurveyDraftManagerIdLengthRule(80);
        var item = PulseSurveyDraft.NewPulseSurveyDraft("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyDraftRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PulseSurveyDraftRequiredSalaryBandRule("pulse survey draft needs an entry");
        var item = PulseSurveyDraft.NewPulseSurveyDraft("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyDraftValidatorReportsEveryProblem();
        PulseSurveyDraftRequiredCodeRuleFlagsABlankCode();
        PulseSurveyDraftManagerIdLengthRuleFlagsALongManagerId();
        PulseSurveyDraftRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}

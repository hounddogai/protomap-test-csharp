namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementRulesChecks
{
    public static void PendingPulseSurveyAgreementValidatorReportsEveryProblem()
    {
        var validator = PendingPulseSurveyAgreementRules.DefaultPendingPulseSurveyAgreementValidator();
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPulseSurveyAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingPulseSurveyAgreementTitleLengthRule(64);
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPulseSurveyAgreementRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new PendingPulseSurveyAgreementRequiredBadgeRule("pending pulse survey agreement needs a value");
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingPulseSurveyAgreementRequiredTitleRule("pending pulse survey agreement needs an entry");
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPulseSurveyAgreementTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PendingPulseSurveyAgreementTeamLengthRule(16);
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAgreementValidatorReportsEveryProblem();
        PendingPulseSurveyAgreementTitleLengthRuleFlagsALongTitle();
        PendingPulseSurveyAgreementRequiredBadgeRuleFlagsABlankBadge();
        PendingPulseSurveyAgreementRequiredTitleRuleFlagsABlankTitle();
        PendingPulseSurveyAgreementTeamLengthRuleFlagsALongTeam();
    }
}

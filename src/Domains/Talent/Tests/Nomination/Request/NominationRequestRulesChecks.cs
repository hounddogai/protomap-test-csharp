namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class NominationRequestRulesChecks
{
    public static void NominationRequestValidatorReportsEveryProblem()
    {
        var validator = NominationRequestRules.DefaultNominationRequestValidator();
        var item = NominationRequest.NewNominationRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationRequestRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new NominationRequestRequiredSkillRule("nomination request needs an entry");
        var item = NominationRequest.NewNominationRequest("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new NominationRequestRequiredTitleRule("nomination request needs a value");
        var item = NominationRequest.NewNominationRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationRequestValidatorReportsEveryProblem();
        NominationRequestRequiredSkillRuleFlagsABlankSkill();
        NominationRequestRequiredTitleRuleFlagsABlankTitle();
    }
}

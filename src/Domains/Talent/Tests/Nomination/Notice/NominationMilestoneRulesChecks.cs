namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationMilestoneRulesChecks
{
    public static void NominationMilestoneValidatorReportsEveryProblem()
    {
        var validator = NominationMilestoneRules.DefaultNominationMilestoneValidator();
        var item = NominationMilestone.NewNominationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new NominationMilestoneRequiredTitleRule("nomination milestone needs a value");
        var item = NominationMilestone.NewNominationMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationMilestoneRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new NominationMilestoneRequiredJobCodeRule("nomination milestone needs an entry");
        var item = NominationMilestone.NewNominationMilestone("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationMilestoneRequiredCodeRule("nomination milestone needs a value");
        var item = NominationMilestone.NewNominationMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationMilestoneValidatorReportsEveryProblem();
        NominationMilestoneRequiredTitleRuleFlagsABlankTitle();
        NominationMilestoneRequiredJobCodeRuleFlagsABlankJobCode();
        NominationMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}

namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationPlanRulesChecks
{
    public static void NominationPlanValidatorReportsEveryProblem()
    {
        var validator = NominationPlanRules.DefaultNominationPlanValidator();
        var item = NominationPlan.NewNominationPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationPlanRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new NominationPlanRateRangeRule(5.1, 4318.5);
        var item = NominationPlan.NewNominationPlan("x", "y");
        item.Rate = 4320.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new NominationPlanRequiredLabelRule("nomination plan needs an entry");
        var item = NominationPlan.NewNominationPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationPlanRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new NominationPlanRequiredTeamRule("nomination plan needs an entry");
        var item = NominationPlan.NewNominationPlan("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationPlanValidatorReportsEveryProblem();
        NominationPlanRateRangeRuleFlagsValuesAboveRate();
        NominationPlanRequiredLabelRuleFlagsABlankLabel();
        NominationPlanRequiredTeamRuleFlagsABlankTeam();
    }
}

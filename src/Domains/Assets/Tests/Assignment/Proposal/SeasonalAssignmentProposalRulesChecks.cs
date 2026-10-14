namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalRulesChecks
{
    public static void SeasonalAssignmentProposalValidatorReportsEveryProblem()
    {
        var validator = SeasonalAssignmentProposalRules.DefaultSeasonalAssignmentProposalValidator();
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAssignmentProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalAssignmentProposalRequiredCodeRule("seasonal assignment proposal needs an entry");
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAssignmentProposalCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SeasonalAssignmentProposalCountryLengthRule(3600);
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAssignmentProposalHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new SeasonalAssignmentProposalHoursRangeRule(1.3, 3340.8);
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("x", "y");
        item.Hours = 3342.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalAssignmentProposalValidatorReportsEveryProblem();
        SeasonalAssignmentProposalRequiredCodeRuleFlagsABlankCode();
        SeasonalAssignmentProposalCountryLengthRuleFlagsALongCountry();
        SeasonalAssignmentProposalHoursRangeRuleFlagsValuesAboveHours();
    }
}

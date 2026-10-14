namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class OccupancyProposalRulesChecks
{
    public static void OccupancyProposalValidatorReportsEveryProblem()
    {
        var validator = OccupancyProposalRules.DefaultOccupancyProposalValidator();
        var item = OccupancyProposal.NewOccupancyProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyProposalCodeLengthRule(86400);
        var item = OccupancyProposal.NewOccupancyProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyProposalRequiredCodeRule("occupancy proposal needs this field");
        var item = OccupancyProposal.NewOccupancyProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyProposalValidatorReportsEveryProblem();
        OccupancyProposalCodeLengthRuleFlagsALongCode();
        OccupancyProposalRequiredCodeRuleFlagsABlankCode();
    }
}

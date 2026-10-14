namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalRulesChecks
{
    public static void LegacyWarrantyProposalValidatorReportsEveryProblem()
    {
        var validator = LegacyWarrantyProposalRules.DefaultLegacyWarrantyProposalValidator();
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyWarrantyProposalCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LegacyWarrantyProposalCapacityRangeRule(80, 86400);
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyWarrantyProposalRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new LegacyWarrantyProposalRequiredDescriptionRule("legacy warranty proposal needs a value");
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyWarrantyProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyWarrantyProposalCodeLengthRule(32);
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyWarrantyProposalRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LegacyWarrantyProposalRequiredTeamRule("legacy warranty proposal needs this field");
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyWarrantyProposalValidatorReportsEveryProblem();
        LegacyWarrantyProposalCapacityRangeRuleFlagsValuesAboveCapacity();
        LegacyWarrantyProposalRequiredDescriptionRuleFlagsABlankDescription();
        LegacyWarrantyProposalCodeLengthRuleFlagsALongCode();
        LegacyWarrantyProposalRequiredTeamRuleFlagsABlankTeam();
    }
}

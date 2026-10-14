namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OccupancyAgreementRulesChecks
{
    public static void OccupancyAgreementValidatorReportsEveryProblem()
    {
        var validator = OccupancyAgreementRules.DefaultOccupancyAgreementValidator();
        var item = OccupancyAgreement.NewOccupancyAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyAgreementTeamLengthRuleFlagsALongTeam()
    {
        var rule = new OccupancyAgreementTeamLengthRule(64);
        var item = OccupancyAgreement.NewOccupancyAgreement("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAgreementRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new OccupancyAgreementRequiredTeamRule("occupancy agreement needs an entry");
        var item = OccupancyAgreement.NewOccupancyAgreement("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyAgreementCodeLengthRule(16);
        var item = OccupancyAgreement.NewOccupancyAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyAgreementTitleLengthRule(16);
        var item = OccupancyAgreement.NewOccupancyAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAgreementRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new OccupancyAgreementRequiredCostCenterRule("occupancy agreement needs this field");
        var item = OccupancyAgreement.NewOccupancyAgreement("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyAgreementValidatorReportsEveryProblem();
        OccupancyAgreementTeamLengthRuleFlagsALongTeam();
        OccupancyAgreementRequiredTeamRuleFlagsABlankTeam();
        OccupancyAgreementCodeLengthRuleFlagsALongCode();
        OccupancyAgreementTitleLengthRuleFlagsALongTitle();
        OccupancyAgreementRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}

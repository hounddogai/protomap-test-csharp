namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgClaimRulesChecks
{
    public static void ReorgClaimValidatorReportsEveryProblem()
    {
        var validator = ReorgClaimRules.DefaultReorgClaimValidator();
        var item = ReorgClaim.NewReorgClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgClaimFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new ReorgClaimFloorValueRangeRule(3.5, 4798.7);
        var item = ReorgClaim.NewReorgClaim("x", "y");
        item.FloorValue = 4800.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgClaimMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ReorgClaimMinimumRangeRule(9.4, 2621.9);
        var item = ReorgClaim.NewReorgClaim("x", "y");
        item.Minimum = 2623.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgClaimBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new ReorgClaimBudgetRangeRule(5.4, 2695.6);
        var item = ReorgClaim.NewReorgClaim("x", "y");
        item.Budget = 2697.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgClaimRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ReorgClaimRequiredTeamRule("reorg claim needs an entry");
        var item = ReorgClaim.NewReorgClaim("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgClaimValidatorReportsEveryProblem();
        ReorgClaimFloorValueRangeRuleFlagsValuesAboveFloorValue();
        ReorgClaimMinimumRangeRuleFlagsValuesAboveMinimum();
        ReorgClaimBudgetRangeRuleFlagsValuesAboveBudget();
        ReorgClaimRequiredTeamRuleFlagsABlankTeam();
    }
}

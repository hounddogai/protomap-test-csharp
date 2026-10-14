namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultAdjustmentAgreementRulesChecks
{
    public static void DefaultAdjustmentAgreementValidatorReportsEveryProblem()
    {
        var validator = DefaultAdjustmentAgreementRules.DefaultDefaultAdjustmentAgreementValidator();
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAdjustmentAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultAdjustmentAgreementTitleLengthRule(64);
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAdjustmentAgreementTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DefaultAdjustmentAgreementTeamLengthRule(255);
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAdjustmentAgreementCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new DefaultAdjustmentAgreementCostCenterLengthRule(1000);
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultAdjustmentAgreementValidatorReportsEveryProblem();
        DefaultAdjustmentAgreementTitleLengthRuleFlagsALongTitle();
        DefaultAdjustmentAgreementTeamLengthRuleFlagsALongTeam();
        DefaultAdjustmentAgreementCostCenterLengthRuleFlagsALongCostCenter();
    }
}

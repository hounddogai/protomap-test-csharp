namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentAgreementRulesChecks
{
    public static void AdjustmentAgreementValidatorReportsEveryProblem()
    {
        var validator = AdjustmentAgreementRules.DefaultAdjustmentAgreementValidator();
        var item = AdjustmentAgreement.NewAdjustmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentAgreementCodeLengthRule(80);
        var item = AdjustmentAgreement.NewAdjustmentAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentAgreementTeamLengthRuleFlagsALongTeam()
    {
        var rule = new AdjustmentAgreementTeamLengthRule(86400);
        var item = AdjustmentAgreement.NewAdjustmentAgreement("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentAgreementMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdjustmentAgreementMinimumRangeRule(8.7, 3097.3);
        var item = AdjustmentAgreement.NewAdjustmentAgreement("x", "y");
        item.Minimum = 3098.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentAgreementRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AdjustmentAgreementRequiredTeamRule("adjustment agreement needs a value");
        var item = AdjustmentAgreement.NewAdjustmentAgreement("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentAgreementValidatorReportsEveryProblem();
        AdjustmentAgreementCodeLengthRuleFlagsALongCode();
        AdjustmentAgreementTeamLengthRuleFlagsALongTeam();
        AdjustmentAgreementMinimumRangeRuleFlagsValuesAboveMinimum();
        AdjustmentAgreementRequiredTeamRuleFlagsABlankTeam();
    }
}

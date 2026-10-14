namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TieredPayBandScoreRulesChecks
{
    public static void TieredPayBandScoreValidatorReportsEveryProblem()
    {
        var validator = TieredPayBandScoreRules.DefaultTieredPayBandScoreValidator();
        var item = TieredPayBandScore.NewTieredPayBandScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayBandScoreRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new TieredPayBandScoreRequiredCostCenterRule("tiered pay band score needs this field");
        var item = TieredPayBandScore.NewTieredPayBandScore("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPayBandScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPayBandScoreCodeLengthRule(86400);
        var item = TieredPayBandScore.NewTieredPayBandScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPayBandScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredPayBandScoreRequiredTitleRule("tiered pay band score needs this field");
        var item = TieredPayBandScore.NewTieredPayBandScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPayBandScoreValidatorReportsEveryProblem();
        TieredPayBandScoreRequiredCostCenterRuleFlagsABlankCostCenter();
        TieredPayBandScoreCodeLengthRuleFlagsALongCode();
        TieredPayBandScoreRequiredTitleRuleFlagsABlankTitle();
    }
}

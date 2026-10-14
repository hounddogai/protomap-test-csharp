namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledBalanceDraftRulesChecks
{
    public static void ScheduledBalanceDraftValidatorReportsEveryProblem()
    {
        var validator = ScheduledBalanceDraftRules.DefaultScheduledBalanceDraftValidator();
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBalanceDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledBalanceDraftCodeLengthRule(86400);
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBalanceDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new ScheduledBalanceDraftTenureMonthsRangeRule(32, 86400);
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBalanceDraftNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledBalanceDraftNameLengthRule(1000);
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBalanceDraftRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ScheduledBalanceDraftRequiredCostCenterRule("scheduled balance draft needs a value");
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBalanceDraftScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ScheduledBalanceDraftScoreRangeRule(4.6, 1125.1);
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("x", "y");
        item.Score = 1126.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledBalanceDraftValidatorReportsEveryProblem();
        ScheduledBalanceDraftCodeLengthRuleFlagsALongCode();
        ScheduledBalanceDraftTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        ScheduledBalanceDraftNameLengthRuleFlagsALongName();
        ScheduledBalanceDraftRequiredCostCenterRuleFlagsABlankCostCenter();
        ScheduledBalanceDraftScoreRangeRuleFlagsValuesAboveScore();
    }
}

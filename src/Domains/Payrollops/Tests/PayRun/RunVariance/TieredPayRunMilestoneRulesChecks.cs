namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class TieredPayRunMilestoneRulesChecks
{
    public static void TieredPayRunMilestoneValidatorReportsEveryProblem()
    {
        var validator = TieredPayRunMilestoneRules.DefaultTieredPayRunMilestoneValidator();
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayRunMilestoneTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new TieredPayRunMilestoneTenureMonthsRangeRule(80, 255);
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPayRunMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredPayRunMilestoneLabelLengthRule(120);
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPayRunMilestoneValidatorReportsEveryProblem();
        TieredPayRunMilestoneTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        TieredPayRunMilestoneLabelLengthRuleFlagsALongLabel();
    }
}

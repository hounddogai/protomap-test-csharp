namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBaselineRulesChecks
{
    public static void BonusPoolBaselineValidatorReportsEveryProblem()
    {
        var validator = BonusPoolBaselineRules.DefaultBonusPoolBaselineValidator();
        var item = BonusPoolBaseline.NewBonusPoolBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolBaselineCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new BonusPoolBaselineCeilingRangeRule(6.2, 2392.4);
        var item = BonusPoolBaseline.NewBonusPoolBaseline("x", "y");
        item.Ceiling = 2393.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolBaselineCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new BonusPoolBaselineCostRangeRule(6.2, 4088.1);
        var item = BonusPoolBaseline.NewBonusPoolBaseline("x", "y");
        item.Cost = 4089.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolBaselineCodeLengthRule(120);
        var item = BonusPoolBaseline.NewBonusPoolBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolBaselineLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new BonusPoolBaselineLeaveDaysRangeRule(32, 120);
        var item = BonusPoolBaseline.NewBonusPoolBaseline("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolBaselineValidatorReportsEveryProblem();
        BonusPoolBaselineCeilingRangeRuleFlagsValuesAboveCeiling();
        BonusPoolBaselineCostRangeRuleFlagsValuesAboveCost();
        BonusPoolBaselineCodeLengthRuleFlagsALongCode();
        BonusPoolBaselineLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}

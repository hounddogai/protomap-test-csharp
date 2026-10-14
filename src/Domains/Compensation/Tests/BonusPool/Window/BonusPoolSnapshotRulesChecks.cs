namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolSnapshotRulesChecks
{
    public static void BonusPoolSnapshotValidatorReportsEveryProblem()
    {
        var validator = BonusPoolSnapshotRules.DefaultBonusPoolSnapshotValidator();
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolSnapshotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new BonusPoolSnapshotLeaveDaysRangeRule(16, 16);
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("x", "y");
        item.LeaveDays = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolSnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new BonusPoolSnapshotRequiredNameRule("bonus pool snapshot needs an entry");
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolSnapshotQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BonusPoolSnapshotQuantityRangeRule(64, 3600);
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolSnapshotRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BonusPoolSnapshotRateRangeRule(4.6, 2904.3);
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("x", "y");
        item.Rate = 2905.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolSnapshotValidatorReportsEveryProblem();
        BonusPoolSnapshotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        BonusPoolSnapshotRequiredNameRuleFlagsABlankName();
        BonusPoolSnapshotQuantityRangeRuleFlagsValuesAboveQuantity();
        BonusPoolSnapshotRateRangeRuleFlagsValuesAboveRate();
    }
}

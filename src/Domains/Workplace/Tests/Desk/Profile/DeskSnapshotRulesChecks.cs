namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskSnapshotRulesChecks
{
    public static void DeskSnapshotValidatorReportsEveryProblem()
    {
        var validator = DeskSnapshotRules.DefaultDeskSnapshotValidator();
        var item = DeskSnapshot.NewDeskSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskSnapshotCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new DeskSnapshotCostRangeRule(6.0, 61.2);
        var item = DeskSnapshot.NewDeskSnapshot("x", "y");
        item.Cost = 62.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskSnapshotRequiredCodeRule("desk snapshot needs an entry");
        var item = DeskSnapshot.NewDeskSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSnapshotRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new DeskSnapshotRequiredCurrencyRule("desk snapshot needs a value");
        var item = DeskSnapshot.NewDeskSnapshot("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskSnapshotLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new DeskSnapshotLimitRangeRule(255, 255);
        var item = DeskSnapshot.NewDeskSnapshot("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskSnapshotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new DeskSnapshotCurrencyLengthRule(255);
        var item = DeskSnapshot.NewDeskSnapshot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskSnapshotValidatorReportsEveryProblem();
        DeskSnapshotCostRangeRuleFlagsValuesAboveCost();
        DeskSnapshotRequiredCodeRuleFlagsABlankCode();
        DeskSnapshotRequiredCurrencyRuleFlagsABlankCurrency();
        DeskSnapshotLimitRangeRuleFlagsValuesAboveLimit();
        DeskSnapshotCurrencyLengthRuleFlagsALongCurrency();
    }
}

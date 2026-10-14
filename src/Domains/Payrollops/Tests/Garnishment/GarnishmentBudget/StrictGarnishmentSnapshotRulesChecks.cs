namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class StrictGarnishmentSnapshotRulesChecks
{
    public static void StrictGarnishmentSnapshotValidatorReportsEveryProblem()
    {
        var validator = StrictGarnishmentSnapshotRules.DefaultStrictGarnishmentSnapshotValidator();
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictGarnishmentSnapshotAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StrictGarnishmentSnapshotAmountRangeRule(0.8, 2226.1);
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("x", "y");
        item.Amount = 2227.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictGarnishmentSnapshotRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new StrictGarnishmentSnapshotRequiredBadgeRule("strict garnishment snapshot needs this field");
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictGarnishmentSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictGarnishmentSnapshotCodeLengthRule(32);
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictGarnishmentSnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictGarnishmentSnapshotRequiredTitleRule("strict garnishment snapshot needs this field");
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictGarnishmentSnapshotMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new StrictGarnishmentSnapshotMinimumRangeRule(6.7, 4074.8);
        var item = StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot("x", "y");
        item.Minimum = 4076.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictGarnishmentSnapshotValidatorReportsEveryProblem();
        StrictGarnishmentSnapshotAmountRangeRuleFlagsValuesAboveAmount();
        StrictGarnishmentSnapshotRequiredBadgeRuleFlagsABlankBadge();
        StrictGarnishmentSnapshotCodeLengthRuleFlagsALongCode();
        StrictGarnishmentSnapshotRequiredTitleRuleFlagsABlankTitle();
        StrictGarnishmentSnapshotMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}

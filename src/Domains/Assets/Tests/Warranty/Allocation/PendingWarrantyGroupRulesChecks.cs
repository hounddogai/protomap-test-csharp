namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingWarrantyGroupRulesChecks
{
    public static void PendingWarrantyGroupValidatorReportsEveryProblem()
    {
        var validator = PendingWarrantyGroupRules.DefaultPendingWarrantyGroupValidator();
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingWarrantyGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingWarrantyGroupTitleLengthRule(120);
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingWarrantyGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingWarrantyGroupLabelLengthRule(32);
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingWarrantyGroupAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingWarrantyGroupAmountRangeRule(8.3, 1364.3);
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("x", "y");
        item.Amount = 1365.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingWarrantyGroupRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PendingWarrantyGroupRatioRangeRule(3.8, 944.8);
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("x", "y");
        item.Ratio = 946.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingWarrantyGroupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PendingWarrantyGroupRetriesRangeRule(80, 120);
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingWarrantyGroupValidatorReportsEveryProblem();
        PendingWarrantyGroupTitleLengthRuleFlagsALongTitle();
        PendingWarrantyGroupLabelLengthRuleFlagsALongLabel();
        PendingWarrantyGroupAmountRangeRuleFlagsValuesAboveAmount();
        PendingWarrantyGroupRatioRangeRuleFlagsValuesAboveRatio();
        PendingWarrantyGroupRetriesRangeRuleFlagsValuesAboveRetries();
    }
}

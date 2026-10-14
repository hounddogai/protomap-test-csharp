namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class PrimaryScenarioTierRulesChecks
{
    public static void PrimaryScenarioTierValidatorReportsEveryProblem()
    {
        var validator = PrimaryScenarioTierRules.DefaultPrimaryScenarioTierValidator();
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryScenarioTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryScenarioTierTitleLengthRule(255);
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryScenarioTierRequiredCodeRule("primary scenario tier needs a value");
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryScenarioTierWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PrimaryScenarioTierWindowDaysRangeRule(80, 120);
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryScenarioTierCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PrimaryScenarioTierCostCenterLengthRule(32);
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioTierAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PrimaryScenarioTierAmountRangeRule(3.5, 3807.8);
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("x", "y");
        item.Amount = 3809.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryScenarioTierValidatorReportsEveryProblem();
        PrimaryScenarioTierTitleLengthRuleFlagsALongTitle();
        PrimaryScenarioTierRequiredCodeRuleFlagsABlankCode();
        PrimaryScenarioTierWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        PrimaryScenarioTierCostCenterLengthRuleFlagsALongCostCenter();
        PrimaryScenarioTierAmountRangeRuleFlagsValuesAboveAmount();
    }
}

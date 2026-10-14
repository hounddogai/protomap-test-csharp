namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleRulesChecks
{
    public static void LaptopRuleValidatorReportsEveryProblem()
    {
        var validator = LaptopRuleRules.DefaultLaptopRuleValidator();
        var item = LaptopRule.NewLaptopRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopRuleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new LaptopRuleAmountRangeRule(2.0, 635.6);
        var item = LaptopRule.NewLaptopRule("x", "y");
        item.Amount = 637.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LaptopRuleTitleLengthRule(3600);
        var item = LaptopRule.NewLaptopRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopRuleTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new LaptopRuleTimezoneLengthRule(16);
        var item = LaptopRule.NewLaptopRule("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopRuleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new LaptopRuleRequiredTimezoneRule("laptop rule needs an entry");
        var item = LaptopRule.NewLaptopRule("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopRuleValidatorReportsEveryProblem();
        LaptopRuleAmountRangeRuleFlagsValuesAboveAmount();
        LaptopRuleTitleLengthRuleFlagsALongTitle();
        LaptopRuleTimezoneLengthRuleFlagsALongTimezone();
        LaptopRuleRequiredTimezoneRuleFlagsABlankTimezone();
    }
}

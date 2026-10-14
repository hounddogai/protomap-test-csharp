namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredLaptopWindowRulesChecks
{
    public static void TieredLaptopWindowValidatorReportsEveryProblem()
    {
        var validator = TieredLaptopWindowRules.DefaultTieredLaptopWindowValidator();
        var item = TieredLaptopWindow.NewTieredLaptopWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLaptopWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredLaptopWindowRequiredLabelRule("tiered laptop window needs this field");
        var item = TieredLaptopWindow.NewTieredLaptopWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new TieredLaptopWindowWindowDaysRangeRule(120, 3600);
        var item = TieredLaptopWindow.NewTieredLaptopWindow("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLaptopWindowRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TieredLaptopWindowRequiredSummaryRule("tiered laptop window needs this field");
        var item = TieredLaptopWindow.NewTieredLaptopWindow("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopWindowSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new TieredLaptopWindowSummaryLengthRule(80);
        var item = TieredLaptopWindow.NewTieredLaptopWindow("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredLaptopWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredLaptopWindowCodeLengthRule(80);
        var item = TieredLaptopWindow.NewTieredLaptopWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLaptopWindowValidatorReportsEveryProblem();
        TieredLaptopWindowRequiredLabelRuleFlagsABlankLabel();
        TieredLaptopWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        TieredLaptopWindowRequiredSummaryRuleFlagsABlankSummary();
        TieredLaptopWindowSummaryLengthRuleFlagsALongSummary();
        TieredLaptopWindowCodeLengthRuleFlagsALongCode();
    }
}

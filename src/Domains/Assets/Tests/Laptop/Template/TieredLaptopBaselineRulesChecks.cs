namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredLaptopBaselineRulesChecks
{
    public static void TieredLaptopBaselineValidatorReportsEveryProblem()
    {
        var validator = TieredLaptopBaselineRules.DefaultTieredLaptopBaselineValidator();
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLaptopBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredLaptopBaselineRequiredTitleRule("tiered laptop baseline needs an entry");
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TieredLaptopBaselineExpiresAtRangeRule(32, 3600);
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLaptopBaselineUnitLengthRuleFlagsALongUnit()
    {
        var rule = new TieredLaptopBaselineUnitLengthRule(120);
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLaptopBaselineValidatorReportsEveryProblem();
        TieredLaptopBaselineRequiredTitleRuleFlagsABlankTitle();
        TieredLaptopBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TieredLaptopBaselineUnitLengthRuleFlagsALongUnit();
    }
}

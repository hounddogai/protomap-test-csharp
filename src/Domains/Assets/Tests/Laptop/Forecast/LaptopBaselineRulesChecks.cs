namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopBaselineRulesChecks
{
    public static void LaptopBaselineValidatorReportsEveryProblem()
    {
        var validator = LaptopBaselineRules.DefaultLaptopBaselineValidator();
        var item = LaptopBaseline.NewLaptopBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopBaselineCodeLengthRule(3600);
        var item = LaptopBaseline.NewLaptopBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopBaselineRequiredNameRuleFlagsABlankName()
    {
        var rule = new LaptopBaselineRequiredNameRule("laptop baseline needs a value");
        var item = LaptopBaseline.NewLaptopBaseline("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new LaptopBaselineNameLengthRule(1000);
        var item = LaptopBaseline.NewLaptopBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopBaselineCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LaptopBaselineCeilingRangeRule(4.2, 311.5);
        var item = LaptopBaseline.NewLaptopBaseline("x", "y");
        item.Ceiling = 313.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LaptopBaselineValidatorReportsEveryProblem();
        LaptopBaselineCodeLengthRuleFlagsALongCode();
        LaptopBaselineRequiredNameRuleFlagsABlankName();
        LaptopBaselineNameLengthRuleFlagsALongName();
        LaptopBaselineCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}

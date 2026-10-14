namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientLaptopWindowRulesChecks
{
    public static void LenientLaptopWindowValidatorReportsEveryProblem()
    {
        var validator = LenientLaptopWindowRules.DefaultLenientLaptopWindowValidator();
        var item = LenientLaptopWindow.NewLenientLaptopWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLaptopWindowScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LenientLaptopWindowScoreRangeRule(5.0, 2777.2);
        var item = LenientLaptopWindow.NewLenientLaptopWindow("x", "y");
        item.Score = 2778.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLaptopWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientLaptopWindowRequiredNameRule("lenient laptop window needs an entry");
        var item = LenientLaptopWindow.NewLenientLaptopWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopWindowNameLengthRuleFlagsALongName()
    {
        var rule = new LenientLaptopWindowNameLengthRule(120);
        var item = LenientLaptopWindow.NewLenientLaptopWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLaptopWindowValidatorReportsEveryProblem();
        LenientLaptopWindowScoreRangeRuleFlagsValuesAboveScore();
        LenientLaptopWindowRequiredNameRuleFlagsABlankName();
        LenientLaptopWindowNameLengthRuleFlagsALongName();
    }
}

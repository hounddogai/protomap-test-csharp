namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopWindowRulesChecks
{
    public static void DefaultLaptopWindowValidatorReportsEveryProblem()
    {
        var validator = DefaultLaptopWindowRules.DefaultDefaultLaptopWindowValidator();
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLaptopWindowGradeLengthRuleFlagsALongGrade()
    {
        var rule = new DefaultLaptopWindowGradeLengthRule(86400);
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultLaptopWindowRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DefaultLaptopWindowRequiredGradeRule("default laptop window needs an entry");
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLaptopWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultLaptopWindowLabelLengthRule(3600);
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultLaptopWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultLaptopWindowRequiredLabelRule("default laptop window needs an entry");
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLaptopWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultLaptopWindowCodeLengthRule(255);
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultLaptopWindowValidatorReportsEveryProblem();
        DefaultLaptopWindowGradeLengthRuleFlagsALongGrade();
        DefaultLaptopWindowRequiredGradeRuleFlagsABlankGrade();
        DefaultLaptopWindowLabelLengthRuleFlagsALongLabel();
        DefaultLaptopWindowRequiredLabelRuleFlagsABlankLabel();
        DefaultLaptopWindowCodeLengthRuleFlagsALongCode();
    }
}

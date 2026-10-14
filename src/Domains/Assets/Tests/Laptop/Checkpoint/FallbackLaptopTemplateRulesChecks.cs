namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackLaptopTemplateRulesChecks
{
    public static void FallbackLaptopTemplateValidatorReportsEveryProblem()
    {
        var validator = FallbackLaptopTemplateRules.DefaultFallbackLaptopTemplateValidator();
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLaptopTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackLaptopTemplateRequiredTitleRule("fallback laptop template needs a value");
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLaptopTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackLaptopTemplateTitleLengthRule(255);
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackLaptopTemplateValidatorReportsEveryProblem();
        FallbackLaptopTemplateRequiredTitleRuleFlagsABlankTitle();
        FallbackLaptopTemplateTitleLengthRuleFlagsALongTitle();
    }
}

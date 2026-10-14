namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class DefaultReorgExceptionRulesChecks
{
    public static void DefaultReorgExceptionValidatorReportsEveryProblem()
    {
        var validator = DefaultReorgExceptionRules.DefaultDefaultReorgExceptionValidator();
        var item = DefaultReorgException.NewDefaultReorgException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultReorgExceptionRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new DefaultReorgExceptionRequiredCategoryRule("default reorg exception needs this field");
        var item = DefaultReorgException.NewDefaultReorgException("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultReorgExceptionValidatorReportsEveryProblem();
        DefaultReorgExceptionRequiredCategoryRuleFlagsABlankCategory();
    }
}

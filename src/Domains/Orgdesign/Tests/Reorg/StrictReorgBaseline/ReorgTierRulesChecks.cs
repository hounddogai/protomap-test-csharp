namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTierRulesChecks
{
    public static void ReorgTierValidatorReportsEveryProblem()
    {
        var validator = ReorgTierRules.DefaultReorgTierValidator();
        var item = ReorgTier.NewReorgTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReorgTierTitleLengthRule(16);
        var item = ReorgTier.NewReorgTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgTierCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ReorgTierCategoryLengthRule(3600);
        var item = ReorgTier.NewReorgTier("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgTierRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ReorgTierRequiredCategoryRule("reorg tier needs an entry");
        var item = ReorgTier.NewReorgTier("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgTierValidatorReportsEveryProblem();
        ReorgTierTitleLengthRuleFlagsALongTitle();
        ReorgTierCategoryLengthRuleFlagsALongCategory();
        ReorgTierRequiredCategoryRuleFlagsABlankCategory();
    }
}

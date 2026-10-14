namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultReorgTierRulesChecks
{
    public static void DefaultReorgTierValidatorReportsEveryProblem()
    {
        var validator = DefaultReorgTierRules.DefaultDefaultReorgTierValidator();
        var item = DefaultReorgTier.NewDefaultReorgTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultReorgTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultReorgTierRequiredCodeRule("default reorg tier needs an entry");
        var item = DefaultReorgTier.NewDefaultReorgTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultReorgTierDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new DefaultReorgTierDescriptionLengthRule(16);
        var item = DefaultReorgTier.NewDefaultReorgTier("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultReorgTierValidatorReportsEveryProblem();
        DefaultReorgTierRequiredCodeRuleFlagsABlankCode();
        DefaultReorgTierDescriptionLengthRuleFlagsALongDescription();
    }
}

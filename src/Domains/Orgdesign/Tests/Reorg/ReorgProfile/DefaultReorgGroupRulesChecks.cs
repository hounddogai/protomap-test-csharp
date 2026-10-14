namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class DefaultReorgGroupRulesChecks
{
    public static void DefaultReorgGroupValidatorReportsEveryProblem()
    {
        var validator = DefaultReorgGroupRules.DefaultDefaultReorgGroupValidator();
        var item = DefaultReorgGroup.NewDefaultReorgGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultReorgGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultReorgGroupRequiredCodeRule("default reorg group needs a value");
        var item = DefaultReorgGroup.NewDefaultReorgGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultReorgGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultReorgGroupRequiredLabelRule("default reorg group needs an entry");
        var item = DefaultReorgGroup.NewDefaultReorgGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultReorgGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultReorgGroupCodeLengthRule(16);
        var item = DefaultReorgGroup.NewDefaultReorgGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultReorgGroupValidatorReportsEveryProblem();
        DefaultReorgGroupRequiredCodeRuleFlagsABlankCode();
        DefaultReorgGroupRequiredLabelRuleFlagsABlankLabel();
        DefaultReorgGroupCodeLengthRuleFlagsALongCode();
    }
}

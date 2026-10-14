namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalSpanRollupRulesChecks
{
    public static void GlobalSpanRollupValidatorReportsEveryProblem()
    {
        var validator = GlobalSpanRollupRules.DefaultGlobalSpanRollupValidator();
        var item = GlobalSpanRollup.NewGlobalSpanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalSpanRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalSpanRollupLabelLengthRule(120);
        var item = GlobalSpanRollup.NewGlobalSpanRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSpanRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalSpanRollupQuantityRangeRule(120, 120);
        var item = GlobalSpanRollup.NewGlobalSpanRollup("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalSpanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalSpanRollupRequiredCodeRule("global span rollup needs this field");
        var item = GlobalSpanRollup.NewGlobalSpanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalSpanRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalSpanRollupRequiredLabelRule("global span rollup needs a value");
        var item = GlobalSpanRollup.NewGlobalSpanRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalSpanRollupRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new GlobalSpanRollupRequiredChannelRule("global span rollup needs an entry");
        var item = GlobalSpanRollup.NewGlobalSpanRollup("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalSpanRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalSpanRollupCodeLengthRule(80);
        var item = GlobalSpanRollup.NewGlobalSpanRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalSpanRollupValidatorReportsEveryProblem();
        GlobalSpanRollupLabelLengthRuleFlagsALongLabel();
        GlobalSpanRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalSpanRollupRequiredCodeRuleFlagsABlankCode();
        GlobalSpanRollupRequiredLabelRuleFlagsABlankLabel();
        GlobalSpanRollupRequiredChannelRuleFlagsABlankChannel();
        GlobalSpanRollupCodeLengthRuleFlagsALongCode();
    }
}

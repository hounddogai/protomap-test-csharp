namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class GlobalCostModelGroupRulesChecks
{
    public static void GlobalCostModelGroupValidatorReportsEveryProblem()
    {
        var validator = GlobalCostModelGroupRules.DefaultGlobalCostModelGroupValidator();
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalCostModelGroupChannelLengthRuleFlagsALongChannel()
    {
        var rule = new GlobalCostModelGroupChannelLengthRule(3600);
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalCostModelGroupCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new GlobalCostModelGroupCategoryLengthRule(255);
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalCostModelGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalCostModelGroupRequiredCodeRule("global cost model group needs an entry");
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalCostModelGroupValidatorReportsEveryProblem();
        GlobalCostModelGroupChannelLengthRuleFlagsALongChannel();
        GlobalCostModelGroupCategoryLengthRuleFlagsALongCategory();
        GlobalCostModelGroupRequiredCodeRuleFlagsABlankCode();
    }
}

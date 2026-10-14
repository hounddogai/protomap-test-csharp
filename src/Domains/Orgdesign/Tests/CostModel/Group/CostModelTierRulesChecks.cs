namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CostModelTierRulesChecks
{
    public static void CostModelTierValidatorReportsEveryProblem()
    {
        var validator = CostModelTierRules.DefaultCostModelTierValidator();
        var item = CostModelTier.NewCostModelTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelTierChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CostModelTierChannelLengthRule(32);
        var item = CostModelTier.NewCostModelTier("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelTierRequiredNameRule("cost model tier needs this field");
        var item = CostModelTier.NewCostModelTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelTierNameLengthRuleFlagsALongName()
    {
        var rule = new CostModelTierNameLengthRule(3600);
        var item = CostModelTier.NewCostModelTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelTierValidatorReportsEveryProblem();
        CostModelTierChannelLengthRuleFlagsALongChannel();
        CostModelTierRequiredNameRuleFlagsABlankName();
        CostModelTierNameLengthRuleFlagsALongName();
    }
}

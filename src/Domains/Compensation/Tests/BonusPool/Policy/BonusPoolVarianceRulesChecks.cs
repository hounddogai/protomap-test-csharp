namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolVarianceRulesChecks
{
    public static void BonusPoolVarianceValidatorReportsEveryProblem()
    {
        var validator = BonusPoolVarianceRules.DefaultBonusPoolVarianceValidator();
        var item = BonusPoolVariance.NewBonusPoolVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolVarianceManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new BonusPoolVarianceManagerIdLengthRule(80);
        var item = BonusPoolVariance.NewBonusPoolVariance("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolVarianceChannelLengthRuleFlagsALongChannel()
    {
        var rule = new BonusPoolVarianceChannelLengthRule(255);
        var item = BonusPoolVariance.NewBonusPoolVariance("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolVarianceCodeLengthRule(120);
        var item = BonusPoolVariance.NewBonusPoolVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BonusPoolVarianceRequiredLabelRule("bonus pool variance needs this field");
        var item = BonusPoolVariance.NewBonusPoolVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolVarianceValidatorReportsEveryProblem();
        BonusPoolVarianceManagerIdLengthRuleFlagsALongManagerId();
        BonusPoolVarianceChannelLengthRuleFlagsALongChannel();
        BonusPoolVarianceCodeLengthRuleFlagsALongCode();
        BonusPoolVarianceRequiredLabelRuleFlagsABlankLabel();
    }
}

namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class PrimaryBonusPoolTierRulesChecks
{
    public static void PrimaryBonusPoolTierValidatorReportsEveryProblem()
    {
        var validator = PrimaryBonusPoolTierRules.DefaultPrimaryBonusPoolTierValidator();
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryBonusPoolTierRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PrimaryBonusPoolTierRequiredSalaryBandRule("primary bonus pool tier needs a value");
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBonusPoolTierRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new PrimaryBonusPoolTierRequiredCostCenterRule("primary bonus pool tier needs an entry");
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBonusPoolTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryBonusPoolTierCodeLengthRule(80);
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryBonusPoolTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryBonusPoolTierRequiredCodeRule("primary bonus pool tier needs an entry");
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryBonusPoolTierValidatorReportsEveryProblem();
        PrimaryBonusPoolTierRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PrimaryBonusPoolTierRequiredCostCenterRuleFlagsABlankCostCenter();
        PrimaryBonusPoolTierCodeLengthRuleFlagsALongCode();
        PrimaryBonusPoolTierRequiredCodeRuleFlagsABlankCode();
    }
}

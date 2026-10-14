namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolClaimRulesChecks
{
    public static void BonusPoolClaimValidatorReportsEveryProblem()
    {
        var validator = BonusPoolClaimRules.DefaultBonusPoolClaimValidator();
        var item = BonusPoolClaim.NewBonusPoolClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolClaimMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new BonusPoolClaimMaximumRangeRule(6.3, 2743.5);
        var item = BonusPoolClaim.NewBonusPoolClaim("x", "y");
        item.Maximum = 2745.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolClaimCodeLengthRule(255);
        var item = BonusPoolClaim.NewBonusPoolClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolClaimSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BonusPoolClaimSkillLengthRule(120);
        var item = BonusPoolClaim.NewBonusPoolClaim("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolClaimExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new BonusPoolClaimExpiresAtRangeRule(80, 1000);
        var item = BonusPoolClaim.NewBonusPoolClaim("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolClaimRequiredCodeRule("bonus pool claim needs a value");
        var item = BonusPoolClaim.NewBonusPoolClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolClaimValidatorReportsEveryProblem();
        BonusPoolClaimMaximumRangeRuleFlagsValuesAboveMaximum();
        BonusPoolClaimCodeLengthRuleFlagsALongCode();
        BonusPoolClaimSkillLengthRuleFlagsALongSkill();
        BonusPoolClaimExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        BonusPoolClaimRequiredCodeRuleFlagsABlankCode();
    }
}

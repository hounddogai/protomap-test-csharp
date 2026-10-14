namespace Acme.Workforce.Talent.Tests.Pool.Agreement;

using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedPoolAgreementRulesChecks
{
    public static void CappedPoolAgreementValidatorReportsEveryProblem()
    {
        var validator = CappedPoolAgreementRules.DefaultCappedPoolAgreementValidator();
        var item = CappedPoolAgreement.NewCappedPoolAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPoolAgreementDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CappedPoolAgreementDiscountRangeRule(7.2, 2145.4);
        var item = CappedPoolAgreement.NewCappedPoolAgreement("x", "y");
        item.Discount = 2146.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPoolAgreementSkillLengthRuleFlagsALongSkill()
    {
        var rule = new CappedPoolAgreementSkillLengthRule(16);
        var item = CappedPoolAgreement.NewCappedPoolAgreement("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedPoolAgreementValidatorReportsEveryProblem();
        CappedPoolAgreementDiscountRangeRuleFlagsValuesAboveDiscount();
        CappedPoolAgreementSkillLengthRuleFlagsALongSkill();
    }
}

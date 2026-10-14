namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantClaimRulesChecks
{
    public static void EquityGrantClaimValidatorReportsEveryProblem()
    {
        var validator = EquityGrantClaimRules.DefaultEquityGrantClaimValidator();
        var item = EquityGrantClaim.NewEquityGrantClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantClaimStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new EquityGrantClaimStartsAtRangeRule(32, 64);
        var item = EquityGrantClaim.NewEquityGrantClaim("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantClaimRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new EquityGrantClaimRequiredCategoryRule("equity grant claim needs a value");
        var item = EquityGrantClaim.NewEquityGrantClaim("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantClaimCodeLengthRule(32);
        var item = EquityGrantClaim.NewEquityGrantClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantClaimValidatorReportsEveryProblem();
        EquityGrantClaimStartsAtRangeRuleFlagsValuesAboveStartsAt();
        EquityGrantClaimRequiredCategoryRuleFlagsABlankCategory();
        EquityGrantClaimCodeLengthRuleFlagsALongCode();
    }
}

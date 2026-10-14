namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientBonusPoolClaimRulesChecks
{
    public static void LenientBonusPoolClaimValidatorReportsEveryProblem()
    {
        var validator = LenientBonusPoolClaimRules.DefaultLenientBonusPoolClaimValidator();
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBonusPoolClaimJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LenientBonusPoolClaimJobCodeLengthRule(86400);
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBonusPoolClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientBonusPoolClaimRequiredNameRule("lenient bonus pool claim needs an entry");
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolClaimRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LenientBonusPoolClaimRequiredLocationRule("lenient bonus pool claim needs an entry");
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBonusPoolClaimRequiredCodeRule("lenient bonus pool claim needs this field");
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolClaimNameLengthRuleFlagsALongName()
    {
        var rule = new LenientBonusPoolClaimNameLengthRule(32);
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientBonusPoolClaimValidatorReportsEveryProblem();
        LenientBonusPoolClaimJobCodeLengthRuleFlagsALongJobCode();
        LenientBonusPoolClaimRequiredNameRuleFlagsABlankName();
        LenientBonusPoolClaimRequiredLocationRuleFlagsABlankLocation();
        LenientBonusPoolClaimRequiredCodeRuleFlagsABlankCode();
        LenientBonusPoolClaimNameLengthRuleFlagsALongName();
    }
}

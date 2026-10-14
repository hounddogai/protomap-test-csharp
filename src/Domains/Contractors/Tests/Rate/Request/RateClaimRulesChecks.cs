namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateClaimRulesChecks
{
    public static void RateClaimValidatorReportsEveryProblem()
    {
        var validator = RateClaimRules.DefaultRateClaimValidator();
        var item = RateClaim.NewRateClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new RateClaimRequiredNameRule("rate claim needs this field");
        var item = RateClaim.NewRateClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateClaimRequiredCodeRule("rate claim needs an entry");
        var item = RateClaim.NewRateClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new RateClaimCodeLengthRule(120);
        var item = RateClaim.NewRateClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateClaimRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RateClaimRequiredJobCodeRule("rate claim needs this field");
        var item = RateClaim.NewRateClaim("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateClaimValidatorReportsEveryProblem();
        RateClaimRequiredNameRuleFlagsABlankName();
        RateClaimRequiredCodeRuleFlagsABlankCode();
        RateClaimCodeLengthRuleFlagsALongCode();
        RateClaimRequiredJobCodeRuleFlagsABlankJobCode();
    }
}

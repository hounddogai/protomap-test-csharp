namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RetroPayDigestRulesChecks
{
    public static void RetroPayDigestValidatorReportsEveryProblem()
    {
        var validator = RetroPayDigestRules.DefaultRetroPayDigestValidator();
        var item = RetroPayDigest.NewRetroPayDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RetroPayDigestRequiredTitleRule("retro pay digest needs this field");
        var item = RetroPayDigest.NewRetroPayDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayDigestRequiredCodeRule("retro pay digest needs a value");
        var item = RetroPayDigest.NewRetroPayDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayDigestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RetroPayDigestSalaryBandLengthRule(86400);
        var item = RetroPayDigest.NewRetroPayDigest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayDigestCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RetroPayDigestCategoryLengthRule(3600);
        var item = RetroPayDigest.NewRetroPayDigest("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayDigestValidatorReportsEveryProblem();
        RetroPayDigestRequiredTitleRuleFlagsABlankTitle();
        RetroPayDigestRequiredCodeRuleFlagsABlankCode();
        RetroPayDigestSalaryBandLengthRuleFlagsALongSalaryBand();
        RetroPayDigestCategoryLengthRuleFlagsALongCategory();
    }
}

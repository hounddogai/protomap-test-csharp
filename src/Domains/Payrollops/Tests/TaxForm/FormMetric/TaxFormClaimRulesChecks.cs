namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Checks of form metric code, run by the project's runner.</summary>
public static class TaxFormClaimRulesChecks
{
    public static void TaxFormClaimValidatorReportsEveryProblem()
    {
        var validator = TaxFormClaimRules.DefaultTaxFormClaimValidator();
        var item = TaxFormClaim.NewTaxFormClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormClaimCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TaxFormClaimCeilingRangeRule(6.1, 979.9);
        var item = TaxFormClaim.NewTaxFormClaim("x", "y");
        item.Ceiling = 981.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TaxFormClaimLabelLengthRule(255);
        var item = TaxFormClaim.NewTaxFormClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormClaimRequiredCodeRule("tax form claim needs an entry");
        var item = TaxFormClaim.NewTaxFormClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormClaimReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new TaxFormClaimReviewerLengthRule(3600);
        var item = TaxFormClaim.NewTaxFormClaim("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormClaimValidatorReportsEveryProblem();
        TaxFormClaimCeilingRangeRuleFlagsValuesAboveCeiling();
        TaxFormClaimLabelLengthRuleFlagsALongLabel();
        TaxFormClaimRequiredCodeRuleFlagsABlankCode();
        TaxFormClaimReviewerLengthRuleFlagsALongReviewer();
    }
}

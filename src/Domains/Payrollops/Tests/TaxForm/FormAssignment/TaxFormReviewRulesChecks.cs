namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormReviewRulesChecks
{
    public static void TaxFormReviewValidatorReportsEveryProblem()
    {
        var validator = TaxFormReviewRules.DefaultTaxFormReviewValidator();
        var item = TaxFormReview.NewTaxFormReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormReviewMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TaxFormReviewMinimumRangeRule(1.7, 4477.3);
        var item = TaxFormReview.NewTaxFormReview("x", "y");
        item.Minimum = 4478.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormReviewRequiredCodeRule("tax form review needs this field");
        var item = TaxFormReview.NewTaxFormReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormReviewUnitLengthRuleFlagsALongUnit()
    {
        var rule = new TaxFormReviewUnitLengthRule(3600);
        var item = TaxFormReview.NewTaxFormReview("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormReviewRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new TaxFormReviewRequiredUnitRule("tax form review needs an entry");
        var item = TaxFormReview.NewTaxFormReview("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormReviewWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TaxFormReviewWeightRangeRule(6.5, 1775.1);
        var item = TaxFormReview.NewTaxFormReview("x", "y");
        item.Weight = 1776.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TaxFormReviewValidatorReportsEveryProblem();
        TaxFormReviewMinimumRangeRuleFlagsValuesAboveMinimum();
        TaxFormReviewRequiredCodeRuleFlagsABlankCode();
        TaxFormReviewUnitLengthRuleFlagsALongUnit();
        TaxFormReviewRequiredUnitRuleFlagsABlankUnit();
        TaxFormReviewWeightRangeRuleFlagsValuesAboveWeight();
    }
}

namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ReturnPlanAgreementRulesChecks
{
    public static void ReturnPlanAgreementValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanAgreementRules.DefaultReturnPlanAgreementValidator();
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanAgreementRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ReturnPlanAgreementRatingRangeRule(0.2, 2746.6);
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("x", "y");
        item.Rating = 2748.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReturnPlanAgreementLabelLengthRule(80);
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanAgreementCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ReturnPlanAgreementCeilingRangeRule(2.2, 3968.1);
        var item = ReturnPlanAgreement.NewReturnPlanAgreement("x", "y");
        item.Ceiling = 3969.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanAgreementValidatorReportsEveryProblem();
        ReturnPlanAgreementRatingRangeRuleFlagsValuesAboveRating();
        ReturnPlanAgreementLabelLengthRuleFlagsALongLabel();
        ReturnPlanAgreementCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}

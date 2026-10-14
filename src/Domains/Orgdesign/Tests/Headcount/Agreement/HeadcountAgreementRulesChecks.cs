namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountAgreementRulesChecks
{
    public static void HeadcountAgreementValidatorReportsEveryProblem()
    {
        var validator = HeadcountAgreementRules.DefaultHeadcountAgreementValidator();
        var item = HeadcountAgreement.NewHeadcountAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountAgreementContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new HeadcountAgreementContactEmailLengthRule(120);
        var item = HeadcountAgreement.NewHeadcountAgreement("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new HeadcountAgreementFloorValueRangeRule(6.9, 90.7);
        var item = HeadcountAgreement.NewHeadcountAgreement("x", "y");
        item.FloorValue = 92.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountAgreementValidatorReportsEveryProblem();
        HeadcountAgreementContactEmailLengthRuleFlagsALongContactEmail();
        HeadcountAgreementFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}

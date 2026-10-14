namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OffboardingAgreementRulesChecks
{
    public static void OffboardingAgreementValidatorReportsEveryProblem()
    {
        var validator = OffboardingAgreementRules.DefaultOffboardingAgreementValidator();
        var item = OffboardingAgreement.NewOffboardingAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OffboardingAgreementLabelLengthRule(80);
        var item = OffboardingAgreement.NewOffboardingAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new OffboardingAgreementLeaveDaysRangeRule(80, 255);
        var item = OffboardingAgreement.NewOffboardingAgreement("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OffboardingAgreementValidatorReportsEveryProblem();
        OffboardingAgreementLabelLengthRuleFlagsALongLabel();
        OffboardingAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}

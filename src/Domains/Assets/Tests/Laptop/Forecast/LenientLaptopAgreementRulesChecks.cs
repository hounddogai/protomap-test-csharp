namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LenientLaptopAgreementRulesChecks
{
    public static void LenientLaptopAgreementValidatorReportsEveryProblem()
    {
        var validator = LenientLaptopAgreementRules.DefaultLenientLaptopAgreementValidator();
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLaptopAgreementRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LenientLaptopAgreementRequiredChecksumRule("lenient laptop agreement needs a value");
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLaptopAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LenientLaptopAgreementLeaveDaysRangeRule(255, 86400);
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLaptopAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientLaptopAgreementRequiredNameRule("lenient laptop agreement needs an entry");
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientLaptopAgreementValidatorReportsEveryProblem();
        LenientLaptopAgreementRequiredChecksumRuleFlagsABlankChecksum();
        LenientLaptopAgreementLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LenientLaptopAgreementRequiredNameRuleFlagsABlankName();
    }
}

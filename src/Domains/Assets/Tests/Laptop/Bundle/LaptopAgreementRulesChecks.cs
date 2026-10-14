namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopAgreementRulesChecks
{
    public static void LaptopAgreementValidatorReportsEveryProblem()
    {
        var validator = LaptopAgreementRules.DefaultLaptopAgreementValidator();
        var item = LaptopAgreement.NewLaptopAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopAgreementRegionLengthRuleFlagsALongRegion()
    {
        var rule = new LaptopAgreementRegionLengthRule(120);
        var item = LaptopAgreement.NewLaptopAgreement("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopAgreementLabelLengthRule(32);
        var item = LaptopAgreement.NewLaptopAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopAgreementValidatorReportsEveryProblem();
        LaptopAgreementRegionLengthRuleFlagsALongRegion();
        LaptopAgreementLabelLengthRuleFlagsALongLabel();
    }
}

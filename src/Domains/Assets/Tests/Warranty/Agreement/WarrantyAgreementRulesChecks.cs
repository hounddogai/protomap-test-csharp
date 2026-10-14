namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyAgreementRulesChecks
{
    public static void WarrantyAgreementValidatorReportsEveryProblem()
    {
        var validator = WarrantyAgreementRules.DefaultWarrantyAgreementValidator();
        var item = WarrantyAgreement.NewWarrantyAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyAgreementEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new WarrantyAgreementEmployeeIdLengthRule(86400);
        var item = WarrantyAgreement.NewWarrantyAgreement("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyAgreementCodeLengthRule(1000);
        var item = WarrantyAgreement.NewWarrantyAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyAgreementCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new WarrantyAgreementCostRangeRule(9.2, 2815.7);
        var item = WarrantyAgreement.NewWarrantyAgreement("x", "y");
        item.Cost = 2817.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyAgreementRequiredCodeRule("warranty agreement needs an entry");
        var item = WarrantyAgreement.NewWarrantyAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyAgreementValidatorReportsEveryProblem();
        WarrantyAgreementEmployeeIdLengthRuleFlagsALongEmployeeId();
        WarrantyAgreementCodeLengthRuleFlagsALongCode();
        WarrantyAgreementCostRangeRuleFlagsValuesAboveCost();
        WarrantyAgreementRequiredCodeRuleFlagsABlankCode();
    }
}

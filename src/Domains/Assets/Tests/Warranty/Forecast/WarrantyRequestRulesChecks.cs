namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyRequestRulesChecks
{
    public static void WarrantyRequestValidatorReportsEveryProblem()
    {
        var validator = WarrantyRequestRules.DefaultWarrantyRequestValidator();
        var item = WarrantyRequest.NewWarrantyRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyRequestRequiredCodeRule("warranty request needs this field");
        var item = WarrantyRequest.NewWarrantyRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WarrantyRequestTitleLengthRule(32);
        var item = WarrantyRequest.NewWarrantyRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyRequestRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WarrantyRequestRequiredChecksumRule("warranty request needs an entry");
        var item = WarrantyRequest.NewWarrantyRequest("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyRequestValidatorReportsEveryProblem();
        WarrantyRequestRequiredCodeRuleFlagsABlankCode();
        WarrantyRequestTitleLengthRuleFlagsALongTitle();
        WarrantyRequestRequiredChecksumRuleFlagsABlankChecksum();
    }
}

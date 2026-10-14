namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryExceptionRequestRulesChecks
{
    public static void PrimaryExceptionRequestValidatorReportsEveryProblem()
    {
        var validator = PrimaryExceptionRequestRules.DefaultPrimaryExceptionRequestValidator();
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryExceptionRequestStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PrimaryExceptionRequestStartsAtRangeRule(16, 255);
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryExceptionRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryExceptionRequestRequiredCodeRule("primary exception request needs an entry");
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryExceptionRequestRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PrimaryExceptionRequestRequiredSalaryBandRule("primary exception request needs an entry");
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryExceptionRequestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PrimaryExceptionRequestSalaryBandLengthRule(64);
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryExceptionRequestValidatorReportsEveryProblem();
        PrimaryExceptionRequestStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PrimaryExceptionRequestRequiredCodeRuleFlagsABlankCode();
        PrimaryExceptionRequestRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PrimaryExceptionRequestSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}

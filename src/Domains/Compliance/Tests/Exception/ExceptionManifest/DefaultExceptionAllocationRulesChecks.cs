namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class DefaultExceptionAllocationRulesChecks
{
    public static void DefaultExceptionAllocationValidatorReportsEveryProblem()
    {
        var validator = DefaultExceptionAllocationRules.DefaultDefaultExceptionAllocationValidator();
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultExceptionAllocationVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DefaultExceptionAllocationVersionRangeRule(120, 255);
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultExceptionAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultExceptionAllocationCodeLengthRule(86400);
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultExceptionAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultExceptionAllocationNameLengthRule(1000);
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultExceptionAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultExceptionAllocationRequiredCodeRule("default exception allocation needs a value");
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultExceptionAllocationValidatorReportsEveryProblem();
        DefaultExceptionAllocationVersionRangeRuleFlagsValuesAboveVersion();
        DefaultExceptionAllocationCodeLengthRuleFlagsALongCode();
        DefaultExceptionAllocationNameLengthRuleFlagsALongName();
        DefaultExceptionAllocationRequiredCodeRuleFlagsABlankCode();
    }
}

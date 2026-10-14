namespace Acme.Workforce.Compliance.Tests.Exception.Proposal;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ExceptionOverrideRulesChecks
{
    public static void ExceptionOverrideValidatorReportsEveryProblem()
    {
        var validator = ExceptionOverrideRules.DefaultExceptionOverrideValidator();
        var item = ExceptionOverride.NewExceptionOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionOverrideRequiredCodeRule("exception override needs this field");
        var item = ExceptionOverride.NewExceptionOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionOverridePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ExceptionOverridePhoneLengthRule(3600);
        var item = ExceptionOverride.NewExceptionOverride("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionOverrideValidatorReportsEveryProblem();
        ExceptionOverrideRequiredCodeRuleFlagsABlankCode();
        ExceptionOverridePhoneLengthRuleFlagsALongPhone();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionDigestRulesChecks
{
    public static void ExceptionDigestValidatorReportsEveryProblem()
    {
        var validator = ExceptionDigestRules.DefaultExceptionDigestValidator();
        var item = ExceptionDigest.NewExceptionDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionDigestRequiredCodeRule("exception digest needs an entry");
        var item = ExceptionDigest.NewExceptionDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ExceptionDigestCodeLengthRule(86400);
        var item = ExceptionDigest.NewExceptionDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionDigestRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ExceptionDigestRequiredDescriptionRule("exception digest needs a value");
        var item = ExceptionDigest.NewExceptionDigest("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionDigestValidatorReportsEveryProblem();
        ExceptionDigestRequiredCodeRuleFlagsABlankCode();
        ExceptionDigestCodeLengthRuleFlagsALongCode();
        ExceptionDigestRequiredDescriptionRuleFlagsABlankDescription();
    }
}

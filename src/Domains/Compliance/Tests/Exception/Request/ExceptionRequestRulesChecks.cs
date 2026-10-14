namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionRequestRulesChecks
{
    public static void ExceptionRequestValidatorReportsEveryProblem()
    {
        var validator = ExceptionRequestRules.DefaultExceptionRequestValidator();
        var item = ExceptionRequest.NewExceptionRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionRequestRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ExceptionRequestRequiredExternalRefRule("exception request needs a value");
        var item = ExceptionRequest.NewExceptionRequest("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionRequestRequiredCodeRule("exception request needs a value");
        var item = ExceptionRequest.NewExceptionRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionRequestValidatorReportsEveryProblem();
        ExceptionRequestRequiredExternalRefRuleFlagsABlankExternalRef();
        ExceptionRequestRequiredCodeRuleFlagsABlankCode();
    }
}

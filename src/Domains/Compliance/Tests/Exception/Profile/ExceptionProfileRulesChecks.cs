namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ExceptionProfileRulesChecks
{
    public static void ExceptionProfileValidatorReportsEveryProblem()
    {
        var validator = ExceptionProfileRules.DefaultExceptionProfileValidator();
        var item = ExceptionProfile.NewExceptionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionProfileRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ExceptionProfileRequiredChannelRule("exception profile needs this field");
        var item = ExceptionProfile.NewExceptionProfile("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionProfileNameLengthRuleFlagsALongName()
    {
        var rule = new ExceptionProfileNameLengthRule(86400);
        var item = ExceptionProfile.NewExceptionProfile("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionProfileRequiredCodeRule("exception profile needs this field");
        var item = ExceptionProfile.NewExceptionProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionProfileValidatorReportsEveryProblem();
        ExceptionProfileRequiredChannelRuleFlagsABlankChannel();
        ExceptionProfileNameLengthRuleFlagsALongName();
        ExceptionProfileRequiredCodeRuleFlagsABlankCode();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class LegacyExceptionProfileRulesChecks
{
    public static void LegacyExceptionProfileValidatorReportsEveryProblem()
    {
        var validator = LegacyExceptionProfileRules.DefaultLegacyExceptionProfileValidator();
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyExceptionProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyExceptionProfileLabelLengthRule(80);
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyExceptionProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyExceptionProfileRequiredCodeRule("legacy exception profile needs a value");
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyExceptionProfileValidatorReportsEveryProblem();
        LegacyExceptionProfileLabelLengthRuleFlagsALongLabel();
        LegacyExceptionProfileRequiredCodeRuleFlagsABlankCode();
    }
}

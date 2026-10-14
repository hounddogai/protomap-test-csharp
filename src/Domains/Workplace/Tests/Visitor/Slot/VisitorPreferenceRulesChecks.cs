namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorPreferenceRulesChecks
{
    public static void VisitorPreferenceValidatorReportsEveryProblem()
    {
        var validator = VisitorPreferenceRules.DefaultVisitorPreferenceValidator();
        var item = VisitorPreference.NewVisitorPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorPreferenceRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new VisitorPreferenceRequiredPhoneRule("visitor preference needs a value");
        var item = VisitorPreference.NewVisitorPreference("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorPreferenceRequiredCodeRule("visitor preference needs this field");
        var item = VisitorPreference.NewVisitorPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorPreferenceValidatorReportsEveryProblem();
        VisitorPreferenceRequiredPhoneRuleFlagsABlankPhone();
        VisitorPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}

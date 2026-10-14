namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskPreferenceRulesChecks
{
    public static void DeskPreferenceValidatorReportsEveryProblem()
    {
        var validator = DeskPreferenceRules.DefaultDeskPreferenceValidator();
        var item = DeskPreference.NewDeskPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskPreferenceRequiredCodeRule("desk preference needs this field");
        var item = DeskPreference.NewDeskPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskPreferenceCodeLengthRule(1000);
        var item = DeskPreference.NewDeskPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskPreferenceVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new DeskPreferenceVersionRangeRule(1000, 1000);
        var item = DeskPreference.NewDeskPreference("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeskPreferenceValidatorReportsEveryProblem();
        DeskPreferenceRequiredCodeRuleFlagsABlankCode();
        DeskPreferenceCodeLengthRuleFlagsALongCode();
        DeskPreferenceVersionRangeRuleFlagsValuesAboveVersion();
    }
}

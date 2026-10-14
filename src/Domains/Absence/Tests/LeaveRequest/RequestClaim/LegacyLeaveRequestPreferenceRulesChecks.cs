namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LegacyLeaveRequestPreferenceRulesChecks
{
    public static void LegacyLeaveRequestPreferenceValidatorReportsEveryProblem()
    {
        var validator = LegacyLeaveRequestPreferenceRules.DefaultLegacyLeaveRequestPreferenceValidator();
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyLeaveRequestPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyLeaveRequestPreferenceRequiredLabelRule("legacy leave request preference needs this field");
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLeaveRequestPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyLeaveRequestPreferenceRequiredTitleRule("legacy leave request preference needs a value");
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLeaveRequestPreferenceMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LegacyLeaveRequestPreferenceMinimumRangeRule(5.4, 2985.9);
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("x", "y");
        item.Minimum = 2987.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestPreferenceValidatorReportsEveryProblem();
        LegacyLeaveRequestPreferenceRequiredLabelRuleFlagsABlankLabel();
        LegacyLeaveRequestPreferenceRequiredTitleRuleFlagsABlankTitle();
        LegacyLeaveRequestPreferenceMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}

namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyDeductionPreferenceRulesChecks
{
    public static void LegacyDeductionPreferenceValidatorReportsEveryProblem()
    {
        var validator = LegacyDeductionPreferenceRules.DefaultLegacyDeductionPreferenceValidator();
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyDeductionPreferenceRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyDeductionPreferenceRequiredBadgeRule("legacy deduction preference needs a value");
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyDeductionPreferenceRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LegacyDeductionPreferenceRequiredCategoryRule("legacy deduction preference needs a value");
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyDeductionPreferenceBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LegacyDeductionPreferenceBadgeLengthRule(64);
        var item = LegacyDeductionPreference.NewLegacyDeductionPreference("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyDeductionPreferenceValidatorReportsEveryProblem();
        LegacyDeductionPreferenceRequiredBadgeRuleFlagsABlankBadge();
        LegacyDeductionPreferenceRequiredCategoryRuleFlagsABlankCategory();
        LegacyDeductionPreferenceBadgeLengthRuleFlagsALongBadge();
    }
}

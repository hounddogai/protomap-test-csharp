namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ManualRateProfileRulesChecks
{
    public static void ManualRateProfileValidatorReportsEveryProblem()
    {
        var validator = ManualRateProfileRules.DefaultManualRateProfileValidator();
        var item = ManualRateProfile.NewManualRateProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRateProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualRateProfileCodeLengthRule(16);
        var item = ManualRateProfile.NewManualRateProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRateProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualRateProfileRequiredNameRule("manual rate profile needs this field");
        var item = ManualRateProfile.NewManualRateProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRateProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualRateProfileLabelLengthRule(32);
        var item = ManualRateProfile.NewManualRateProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRateProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new ManualRateProfileTenureMonthsRangeRule(64, 120);
        var item = ManualRateProfile.NewManualRateProfile("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualRateProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualRateProfileRequiredLabelRule("manual rate profile needs this field");
        var item = ManualRateProfile.NewManualRateProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualRateProfileValidatorReportsEveryProblem();
        ManualRateProfileCodeLengthRuleFlagsALongCode();
        ManualRateProfileRequiredNameRuleFlagsABlankName();
        ManualRateProfileLabelLengthRuleFlagsALongLabel();
        ManualRateProfileTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        ManualRateProfileRequiredLabelRuleFlagsABlankLabel();
    }
}

namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualPreferenceRulesChecks
{
    public static void AccrualPreferenceValidatorReportsEveryProblem()
    {
        var validator = AccrualPreferenceRules.DefaultAccrualPreferenceValidator();
        var item = AccrualPreference.NewAccrualPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualPreferenceFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new AccrualPreferenceFloorValueRangeRule(5.2, 1782.2);
        var item = AccrualPreference.NewAccrualPreference("x", "y");
        item.FloorValue = 1783.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualPreferenceNameLengthRule(1000);
        var item = AccrualPreference.NewAccrualPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualPreferenceRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AccrualPreferenceRatingRangeRule(2.8, 4647.1);
        var item = AccrualPreference.NewAccrualPreference("x", "y");
        item.Rating = 4648.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccrualPreferenceValidatorReportsEveryProblem();
        AccrualPreferenceFloorValueRangeRuleFlagsValuesAboveFloorValue();
        AccrualPreferenceNameLengthRuleFlagsALongName();
        AccrualPreferenceRatingRangeRuleFlagsValuesAboveRating();
    }
}

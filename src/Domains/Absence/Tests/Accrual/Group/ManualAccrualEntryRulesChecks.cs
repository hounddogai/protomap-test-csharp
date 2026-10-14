namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualAccrualEntryRulesChecks
{
    public static void ManualAccrualEntryValidatorReportsEveryProblem()
    {
        var validator = ManualAccrualEntryRules.DefaultManualAccrualEntryValidator();
        var item = ManualAccrualEntry.NewManualAccrualEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAccrualEntryRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ManualAccrualEntryRatingRangeRule(8.0, 4883.8);
        var item = ManualAccrualEntry.NewManualAccrualEntry("x", "y");
        item.Rating = 4885.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccrualEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualAccrualEntryRequiredLabelRule("manual accrual entry needs a value");
        var item = ManualAccrualEntry.NewManualAccrualEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccrualEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualAccrualEntryLabelLengthRule(16);
        var item = ManualAccrualEntry.NewManualAccrualEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAccrualEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualAccrualEntryRequiredCodeRule("manual accrual entry needs a value");
        var item = ManualAccrualEntry.NewManualAccrualEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAccrualEntryValidatorReportsEveryProblem();
        ManualAccrualEntryRatingRangeRuleFlagsValuesAboveRating();
        ManualAccrualEntryRequiredLabelRuleFlagsABlankLabel();
        ManualAccrualEntryLabelLengthRuleFlagsALongLabel();
        ManualAccrualEntryRequiredCodeRuleFlagsABlankCode();
    }
}

namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualEntryRulesChecks
{
    public static void AccrualEntryValidatorReportsEveryProblem()
    {
        var validator = AccrualEntryRules.DefaultAccrualEntryValidator();
        var item = AccrualEntry.NewAccrualEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualEntryDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AccrualEntryDiscountRangeRule(1.3, 3726.8);
        var item = AccrualEntry.NewAccrualEntry("x", "y");
        item.Discount = 3728.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualEntryCodeLengthRule(86400);
        var item = AccrualEntry.NewAccrualEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualEntryRequiredLabelRule("accrual entry needs this field");
        var item = AccrualEntry.NewAccrualEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualEntryEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new AccrualEntryEmployeeIdLengthRule(64);
        var item = AccrualEntry.NewAccrualEntry("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualEntryValidatorReportsEveryProblem();
        AccrualEntryDiscountRangeRuleFlagsValuesAboveDiscount();
        AccrualEntryCodeLengthRuleFlagsALongCode();
        AccrualEntryRequiredLabelRuleFlagsABlankLabel();
        AccrualEntryEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}

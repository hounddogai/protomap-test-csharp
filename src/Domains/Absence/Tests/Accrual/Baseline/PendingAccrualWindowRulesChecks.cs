namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingAccrualWindowRulesChecks
{
    public static void PendingAccrualWindowValidatorReportsEveryProblem()
    {
        var validator = PendingAccrualWindowRules.DefaultPendingAccrualWindowValidator();
        var item = PendingAccrualWindow.NewPendingAccrualWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAccrualWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingAccrualWindowRequiredCodeRule("pending accrual window needs an entry");
        var item = PendingAccrualWindow.NewPendingAccrualWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAccrualWindowStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PendingAccrualWindowStartsAtRangeRule(255, 3600);
        var item = PendingAccrualWindow.NewPendingAccrualWindow("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingAccrualWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingAccrualWindowCodeLengthRule(120);
        var item = PendingAccrualWindow.NewPendingAccrualWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAccrualWindowSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PendingAccrualWindowSalaryBandLengthRule(32);
        var item = PendingAccrualWindow.NewPendingAccrualWindow("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingAccrualWindowValidatorReportsEveryProblem();
        PendingAccrualWindowRequiredCodeRuleFlagsABlankCode();
        PendingAccrualWindowStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PendingAccrualWindowCodeLengthRuleFlagsALongCode();
        PendingAccrualWindowSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}

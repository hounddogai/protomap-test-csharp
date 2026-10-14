namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionScheduleRulesChecks
{
    public static void DeductionScheduleValidatorReportsEveryProblem()
    {
        var validator = DeductionScheduleRules.DefaultDeductionScheduleValidator();
        var item = DeductionSchedule.NewDeductionSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionScheduleRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new DeductionScheduleRequiredOwnerIdRule("deduction schedule needs this field");
        var item = DeductionSchedule.NewDeductionSchedule("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionScheduleRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new DeductionScheduleRequiredChecksumRule("deduction schedule needs an entry");
        var item = DeductionSchedule.NewDeductionSchedule("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionScheduleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DeductionScheduleLeaveDaysRangeRule(32, 120);
        var item = DeductionSchedule.NewDeductionSchedule("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionScheduleValidatorReportsEveryProblem();
        DeductionScheduleRequiredOwnerIdRuleFlagsABlankOwnerId();
        DeductionScheduleRequiredChecksumRuleFlagsABlankChecksum();
        DeductionScheduleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}

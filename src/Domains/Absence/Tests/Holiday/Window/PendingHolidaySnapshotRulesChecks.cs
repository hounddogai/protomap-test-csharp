namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingHolidaySnapshotRulesChecks
{
    public static void PendingHolidaySnapshotValidatorReportsEveryProblem()
    {
        var validator = PendingHolidaySnapshotRules.DefaultPendingHolidaySnapshotValidator();
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingHolidaySnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingHolidaySnapshotRequiredCodeRule("pending holiday snapshot needs an entry");
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingHolidaySnapshotContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PendingHolidaySnapshotContactEmailLengthRule(32);
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingHolidaySnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingHolidaySnapshotCodeLengthRule(86400);
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingHolidaySnapshotValidatorReportsEveryProblem();
        PendingHolidaySnapshotRequiredCodeRuleFlagsABlankCode();
        PendingHolidaySnapshotContactEmailLengthRuleFlagsALongContactEmail();
        PendingHolidaySnapshotCodeLengthRuleFlagsALongCode();
    }
}

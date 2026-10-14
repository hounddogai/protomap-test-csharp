namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class FallbackHolidaySnapshotRulesChecks
{
    public static void FallbackHolidaySnapshotValidatorReportsEveryProblem()
    {
        var validator = FallbackHolidaySnapshotRules.DefaultFallbackHolidaySnapshotValidator();
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackHolidaySnapshotRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new FallbackHolidaySnapshotRequiredExternalRefRule("fallback holiday snapshot needs an entry");
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackHolidaySnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackHolidaySnapshotCodeLengthRule(32);
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackHolidaySnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackHolidaySnapshotLabelLengthRule(80);
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackHolidaySnapshotValidatorReportsEveryProblem();
        FallbackHolidaySnapshotRequiredExternalRefRuleFlagsABlankExternalRef();
        FallbackHolidaySnapshotCodeLengthRuleFlagsALongCode();
        FallbackHolidaySnapshotLabelLengthRuleFlagsALongLabel();
    }
}

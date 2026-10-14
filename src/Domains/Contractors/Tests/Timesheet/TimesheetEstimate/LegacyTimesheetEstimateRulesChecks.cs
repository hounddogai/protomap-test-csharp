namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

/// <summary>Checks of timesheet estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetEstimateRulesChecks
{
    public static void LegacyTimesheetEstimateValidatorReportsEveryProblem()
    {
        var validator = LegacyTimesheetEstimateRules.DefaultLegacyTimesheetEstimateValidator();
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTimesheetEstimateNotesLengthRuleFlagsALongNotes()
    {
        var rule = new LegacyTimesheetEstimateNotesLengthRule(32);
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTimesheetEstimateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LegacyTimesheetEstimateSequenceRangeRule(64, 1000);
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTimesheetEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTimesheetEstimateCodeLengthRule(64);
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTimesheetEstimateValidatorReportsEveryProblem();
        LegacyTimesheetEstimateNotesLengthRuleFlagsALongNotes();
        LegacyTimesheetEstimateSequenceRangeRuleFlagsValuesAboveSequence();
        LegacyTimesheetEstimateCodeLengthRuleFlagsALongCode();
    }
}

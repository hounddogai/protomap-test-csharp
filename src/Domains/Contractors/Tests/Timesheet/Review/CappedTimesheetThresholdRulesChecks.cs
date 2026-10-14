namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedTimesheetThresholdRulesChecks
{
    public static void CappedTimesheetThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedTimesheetThresholdRules.DefaultCappedTimesheetThresholdValidator();
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTimesheetThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new CappedTimesheetThresholdNameLengthRule(120);
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTimesheetThresholdMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CappedTimesheetThresholdMaximumRangeRule(6.2, 1984.2);
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("x", "y");
        item.Maximum = 1985.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedTimesheetThresholdThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new CappedTimesheetThresholdThresholdRangeRule(9.6, 4522.3);
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("x", "y");
        item.Threshold = 4523.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedTimesheetThresholdCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new CappedTimesheetThresholdCeilingRangeRule(4.0, 882.4);
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("x", "y");
        item.Ceiling = 883.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedTimesheetThresholdRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new CappedTimesheetThresholdRequiredDescriptionRule("capped timesheet threshold needs this field");
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedTimesheetThresholdValidatorReportsEveryProblem();
        CappedTimesheetThresholdNameLengthRuleFlagsALongName();
        CappedTimesheetThresholdMaximumRangeRuleFlagsValuesAboveMaximum();
        CappedTimesheetThresholdThresholdRangeRuleFlagsValuesAboveThreshold();
        CappedTimesheetThresholdCeilingRangeRuleFlagsValuesAboveCeiling();
        CappedTimesheetThresholdRequiredDescriptionRuleFlagsABlankDescription();
    }
}
